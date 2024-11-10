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
using OPZBackup.ResponseHandlers;
using OPZBackup.ResponseHandlers.Backup;
using OPZBackup.Services;
using OPZBackup.Services.Backup;
using OPZBackup.Services.Utils;
using Serilog;
using Serilog.Events;

namespace OPZBackup;

public abstract class StartupBase
{
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

    protected record StartupServices(
        InteractionHandler interactionHandler,
        DiscordSocketClient SocketClient,
        InteractionService Commands,
        ILogger<Program> Logger
    );

    protected static StartupServices GetStartupServices(IServiceProvider services)
    {
        return new StartupServices(
            services.GetRequiredService<InteractionHandler>(),
            services.GetRequiredService<DiscordSocketClient>(),
            services.GetRequiredService<InteractionService>(),
            services.GetRequiredService<ILogger<Program>>()
        );
    }

    protected static void ConfigureApplication(StartupServices services)
    {
        //TODO Refactor this 
        services.SocketClient.Log += msg =>
            services.Logger.RichLogAsync(EnhancedLogger.ParseLogLevel(msg.Severity), msg.Exception,
                "{subject} " + msg.Message,
                "BOT:");

        services.Commands.Log += msg =>
            services.Logger.RichLogAsync(EnhancedLogger.ParseLogLevel(msg.Severity), msg.Exception,
                "{subject} " + msg.Message,
                "COMMAND:");

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
            s.AddDbContext<MyDbContext>(db => db.UseSqlite("Data Source=opzbackup.db"))
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
                .AddScoped<BackupService>()
                .AddScoped<MessageProcessor>()
                .AddScoped<MessageFetcher>()
                .AddScoped<BackupModule>()
                .AddScoped<AttachmentDownloader>()
                .AddScoped<DirCompressor>()
                .AddScoped<ServiceResponseHandlerFactory>()
                .AddScoped<ModuleResponseHandler>()
                .AddTransient<BackupContextFactory>()
                .AddTransient<ResponseBuilder>()
                .RemoveAll<IHttpMessageHandlerBuilderFilter>();
        });
    }
}