using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Context;
using OPZBot.Logging;
using OPZBot.Services;
using OPZBot.Services.MessageBackup;
using Serilog;
using Serilog.Events;
using RunMode = Discord.Interactions.RunMode;

namespace OPZBot;

public class Program
{
    public const string Ver = "v0.1";
    public static DateTime SessionDate { get; } = DateTime.Now;

    public static Task Main(string[] args)
    {
        return new Program().MainAsync(args);
    }

    private async Task MainAsync(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting host");
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();

            using var host = Host.CreateDefaultBuilder()
                .ConfigureServices((ctx, sc) => ConfigureBotServices(ctx, sc, config))
                .UseSerilog((_, _, cfg)
                        => cfg
                            .Enrich.FromLogContext()
                            .MinimumLevel.Information()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .WriteTo.Console(),
                    preserveStaticLogger: true
                )
                .Build();
            await RunAsync(host);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            var sessionDate = $"{SessionDate:dd.MM.yyyy_H.mm.ss}";
            using (var sw = new StreamWriter(Path.Combine(AppContext.BaseDirectory, $"crashreport_{sessionDate}.log")))
            {
                sw.WriteLine("Host terminated with error:\n" + ex);
            }
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private async Task RunAsync(IHost host)
    {
        using var serviceScope = host.Services.CreateScope();
        var provider = serviceScope.ServiceProvider;

        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
        var client = provider.GetRequiredService<DiscordSocketClient>();
        var sCommands = provider.GetRequiredService<InteractionService>();
        var config = provider.GetRequiredService<IConfigurationRoot>();
        var logger = provider.GetRequiredService<ILogger<Program>>();

        client.Log += msg
            => logger.RichLogAsync(LogUtil.ParseLogLevel(msg.Severity), msg.Exception, "BOT: " + msg.Message);

        sCommands.Log += msg
            => logger.RichLogAsync(LogUtil.ParseLogLevel(msg.Severity), msg.Exception, "COMMAND: " + msg.Message);

        client.Ready += async ()
            => await sCommands.RegisterCommandsToGuildAsync(ulong.Parse(config["testGuild"]!));

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
            //.LogTo(Console.WriteLine).EnableSensitiveDataLogging().EnableDetailedErrors())
            .AddConfiguredCacheManager()
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
            .AddSingleton(_ =>
                new CommandService()) //TODO is it really necessary to be a expression instead of generic?
            .AddSingleton(config)
            .AddSingleton<Mapper>()
            .AddScoped<IMessageFetcher, MessageFetcher>()
            .AddScoped<IBackupMessageProcessor, MessageProcessor>()
            .AddScoped<BackupMessageService>()
            .AddScoped<ResponseHandler>()
            .AddScoped<ResponseBuilder>()
            .AddSingleton<LoggingWrapper>()
            ;
    }
}