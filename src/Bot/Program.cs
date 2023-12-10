using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPZBot.DataAccess;
using RunMode = Discord.Interactions.RunMode;

namespace OPZBot.Bot;

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
            .ConfigureServices((_,services) => services.AddDataAccess(config["connectionString"]))
            .ConfigureServices((_,services) => services.AddSingleton(config))
            .ConfigureServices(SetServices)
            
            .Build();

        
        await RunAsync(host);
    }

    public static void ConfigureDomainServices(IServiceCollection services)
    {
        
    }
    
    private void SetServices(HostBuilderContext context, IServiceCollection services)
    {
        services
            .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All,
                AlwaysDownloadUsers = true
            }))
            .AddSingleton(provider => new InteractionService(
                provider.GetRequiredService<DiscordSocketClient>(),
                new InteractionServiceConfig()
                {
                    DefaultRunMode = RunMode.Async
                }))
            .AddSingleton<InteractionHandler>()
            .AddSingleton(_ => new CommandService())
            .AddScoped<BackupService>();
    }
    
    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        var provider = serviceScope.ServiceProvider;
        
        var client = provider.GetRequiredService<DiscordSocketClient>();
        var sCommands = provider.GetRequiredService<InteractionService>();
        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
        var config = provider.GetRequiredService<IConfigurationRoot>();
        
        
        client.Log += async msg => Console.WriteLine(msg.Message);
        sCommands.Log += async msg => Console.WriteLine(msg.Message);
        
        
        client.Ready += async () =>
        {
            Console.WriteLine("Bot ready!");
            await sCommands.RegisterCommandsToGuildAsync(UInt64.Parse(config["testGuild"]));
        };

        await client.LoginAsync(TokenType.Bot, config["token"]);
        await client.StartAsync();
        
        
        await Task.Delay(-1);
    }
}