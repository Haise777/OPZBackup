using AnsiStyles;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using OPZBackup.Data;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.Modules;
using OPZBackup.ResponseHandlers.Backup;
using OPZBackup.Services;
using OPZBackup.Services.Backup;
using OPZBackup.Services.Utils;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace OPZBackup;

public abstract class StartupBase
{
    //TODO-4 Rethink the whole startup class organization
    protected static Task<bool> CreateDbFileIfNotExists(IHost host)
    {
        using var serviceScope = host.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<MyDbContext>();
        return context.Database.EnsureCreatedAsync();
    }

    protected static void ConfigureStaticLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
    }

    protected static StartupServices GetStartupServices(IServiceProvider services)
    {
        return new StartupServices(
            services.GetRequiredService<InteractionHandler>(),
            services.GetRequiredService<DiscordSocketClient>(),
            services.GetRequiredService<InteractionService>(),
            services.GetRequiredService<ILogger>()
        );
    }

    protected static void ConfigureApplication(StartupServices services)
    {
        var c = StringStyle.Foreground.BrightBlue;
        var r = StringStyle.Reset;
        var logger = services.Logger.ForContext("System", $"{c}DiscordNet{r}");

        services.SocketClient.Log += msg =>
            Task.Run(() => logger.Write(EnhancedLogger.ParseLogLevel(msg.Severity), msg.Exception, msg.Message));

        services.Commands.Log += msg =>
            Task.Run(() => logger.Write(EnhancedLogger.ParseLogLevel(msg.Severity), msg.Exception, msg.Message));

        services.SocketClient.Ready += async () =>
        {
            if (Dev.IsDebug)
            {
                if (Dev.TestGuildId == default)
                    throw new ApplicationException("TestGuildId is not defined");

                await services.Commands.RegisterCommandsToGuildAsync(Dev.TestGuildId);
            }
            else
            {
                await services.Commands.RegisterCommandsGloballyAsync();
            }

            App.SetBotUserId(services.SocketClient.CurrentUser.Id);
        };
    }

    protected static void ConfigureServices(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((_, s) =>
        {
            s.AddDbContext<MyDbContext>(db => db.UseSqlite($"Data Source={App.BaseBackupPath}/opzbackup.db"))
                .AddHttpClient()
                .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.All,
                    AlwaysDownloadUsers = true
                }))
                .AddSingleton(p =>
                    new InteractionService(p.GetRequiredService<DiscordSocketClient>(),
                        new InteractionServiceConfig
                        {
                            DefaultRunMode = RunMode.Async
                        }
                    )
                )
                .AddSingleton<Mapper>()
                .AddScoped<InteractionHandler>()
                .AddScoped<BackupLogger>()
                .AddScoped<BackupService>()
                .AddScoped<MessageProcessor>()
                .AddScoped<MessageFetcher>()
                .AddScoped<BackupModule>()
                .AddScoped<AttachmentDownloader>()
                .AddScoped<DirCompressor>()
                .AddScoped<ServiceResponseHandlerFactory>()
                .AddScoped<ModuleResponseHandler>()
                .AddScoped<FileCleaner>()
                .AddTransient<BackupContextFactory>()
                .AddTransient<ResponseBuilder>()
                .RemoveAll<IHttpMessageHandlerBuilderFilter>();
        });
    }

    protected record StartupServices(
        InteractionHandler interactionHandler,
        DiscordSocketClient SocketClient,
        InteractionService Commands,
        ILogger Logger
    );
}