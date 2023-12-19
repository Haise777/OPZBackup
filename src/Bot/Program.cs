using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPZBot.Logging;
using Serilog;
using Serilog.Events;

namespace OPZBot;

public class Program
{
    public const string APP_VER = "0.1";
    public static bool RunWithCooldowns { get; private set; }
    public static DateTime SessionTime { get; } = DateTime.Now;
    public static string FileBackupPath { get; } = @$"{AppContext.BaseDirectory}Backup\Files";
    public static ulong MainAdminRoleId { get; private set; }
    public static ulong BotUserId { get; private set; }

    public static Task Main(string[] args)
    {
        return new Program().MainAsync(args);
    }

    private async Task MainAsync(string[] args)
    {
        new StartupConfig().Initialize();

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
            MainAdminRoleId = config.GetValue<ulong>("MainAdminRoleId");
            RunWithCooldowns = config.GetValue<bool>("RunWithCooldowns");
            if (!RunWithCooldowns) Log.Warning("Running without cooldowns!");

            using var host = Host.CreateDefaultBuilder()
                .ConfigureBotServices(config)
                .UseSerilog((_, _, cfg)
                        => cfg
                            .Enrich.FromLogContext()
                            .MinimumLevel.Information()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .WriteTo.Console(),
                    true
                )
                .Build();

            await RunAsync(host);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            var sessionDate = $"{SessionTime:dd.MM.yyyy_HH.mm.ss}";
            await using (var sw = new StreamWriter(Path.Combine(AppContext.BaseDirectory,
                             $"crashreport_{sessionDate}.log")))
            {
                await sw.WriteLineAsync("Host terminated with error:\n" + ex);
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
            => logger.RichLogAsync(LogUtil.ParseLogLevel(msg.Severity), msg.Exception, "{subject} " + msg.Message,
                "BOT:");

        sCommands.Log += msg
            => logger.RichLogAsync(LogUtil.ParseLogLevel(msg.Severity), msg.Exception, "{subject} " + msg.Message,
                "COMMAND:");

        client.Ready += async () =>
        {
#if DEBUG
            await sCommands.RegisterCommandsToGuildAsync(config.GetValue<ulong>("TestGuildId"));
#else
            await sCommands.RegisterCommandsGloballyAsync();
#endif
            BotUserId = client.CurrentUser.Id;
            client.ValidateConfigIds(config);
        };
        
        
        await client.LoginAsync(TokenType.Bot, config["Token"]);
        await client.StartAsync();
        await Task.Delay(-1);
    }
}