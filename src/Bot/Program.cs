using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.Services;
using OPZBot.Services.MessageBackup;
using RunMode = Discord.Interactions.RunMode;

namespace OPZBot;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync(args);

    private async Task MainAsync(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("config.json")
            .Build();

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, sc)
                => ConfigureBotServices(ctx, sc, config))
            .Build();

        await RunAsync(host);
    }

    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        var provider = serviceScope.ServiceProvider;

        var client = provider.GetRequiredService<DiscordSocketClient>();
        var sCommands = provider.GetRequiredService<InteractionService>();
        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
        var config = provider.GetRequiredService<IConfigurationRoot>();


        client.Log += msg => Task.Run(() => Console.WriteLine(msg.Message));
        sCommands.Log += msg => Task.Run(() => Console.WriteLine(msg.Message));


        client.Ready += async () =>
        {
            Console.WriteLine("Bot ready!");
            await sCommands.RegisterCommandsToGuildAsync(ulong.Parse(config["testGuild"]!));
        };

        await client.LoginAsync(TokenType.Bot, config["token"]);
        await client.StartAsync();


        await Task.Delay(-1);
    }

    private void ConfigureBotServices(HostBuilderContext context, IServiceCollection services,
        IConfigurationRoot config)
    {
        services
            .AddDbContext<MyDbContext>(options
                => options.UseMySql(config["connectionString"], ServerVersion.Parse("8.0.34-mysql")))
            .AddSingleton(provider =>
                new IdCacheManager(
                    new DataCache<ulong>().AddAsync(provider.GetRequiredService<MyDbContext>()
                        .Users.Select(u => u.Id).ToList()).Result,
                    new DataCache<ulong>().AddAsync(provider.GetRequiredService<MyDbContext>()
                        .Users.Select(u => u.Id).ToList()).Result
                )
            )
            .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                AlwaysDownloadUsers = true
            }))
            .AddSingleton(provider =>
                new InteractionService(provider.GetRequiredService<DiscordSocketClient>(),
                    new InteractionServiceConfig
                    {
                        DefaultRunMode = RunMode.Async
                    }
                )
            )
            .AddSingleton<InteractionHandler>()
            .AddSingleton(_ => new CommandService())
            .AddSingleton(config)
            .AddSingleton<Mapper>()
            .AddScoped<BackupService>()
            .AddScoped<IMessageFetcher, MessageFetcher>()
            .AddScoped<IBackupMessageProcessor, BackupMessageProcessor>()
            ;
    }
}