using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPZBackup.Data;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.Modules;
using OPZBackup.ResponseHandlers;
using OPZBackup.Services;
using OPZBackup.Services.Utils;
using Serilog;
using Serilog.Events;

namespace OPZBackup;

public abstract class StartupBase
{
    protected static Task CreateDbFileIfNotExists(IHost host)
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
#if DEBUG
            await services.Commands.RegisterCommandsToGuildAsync(AppInfo.TestGuildId);
#else
            await sCommands.RegisterCommandsGloballyAsync();
#endif
            AppInfo.SetBotUserId(services.SocketClient.CurrentUser.Id);
        };
    }

    protected static void ConfigureServices(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((_, s) =>
        {
            s.AddDbContext<MyDbContext>(db => db.UseSqlite("Data Source=opzbackup.db"));
            
            s.AddSingleton<Mapper>();
            s.AddScoped<BackupService>();
            s.AddScoped<MessageProcessor>();
            s.AddScoped<MessageFetcher>();
            s.AddScoped<BackupModule>();
            s.AddScoped<AttachmentDownloader>();
            s.AddScoped<DirCompressor>();
            s.AddTransient<BackupContextFactory>();
            s.AddTransient<BackupResponseHandlerFactory>();
            s.AddTransient<EmbedResponseBuilder>();
        });
    }
}