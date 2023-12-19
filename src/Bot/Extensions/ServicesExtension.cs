using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using OPZBot;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.Services;
using OPZBot.Services.MessageBackup;
using OPZBot.Services.MessageBackup.FileBackup;
using RunMode = Discord.Interactions.RunMode;

namespace OPZBot.Extensions;

public static class ServicesExtension
{
    public static IHostBuilder ConfigureBotServices(this IHostBuilder host, IConfigurationRoot config)
    {
        host.ConfigureServices((ctx, services) => services
            .AddDbContext<MyDbContext>(options
                => options.UseMySql(config["connectionString"], ServerVersion.Parse("8.0.34-mysql")))
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
            .AddSingleton(config)
            .AddSingleton<InteractionHandler>()
            .AddSingleton<CommandService>()
            .AddSingleton<Mapper>()
            .AddSingleton<LoggingWrapper>()
            .AddScoped<IMessageFetcher, MessageFetcher>()
            .AddScoped<IBackupMessageProcessor, MessageProcessor>()
            .AddScoped<IBackupMessageService, BackupMessageService>()
            .AddScoped<IResponseHandler, ResponseHandler>()
            .AddScoped<IFileBackupService, FileBackupService>()
            .AddScoped<ResponseBuilder>()
            .AddHttpClient()
            .RemoveAll<IHttpMessageHandlerBuilderFilter>()
        );

        return host;
    }

    public static IServiceCollection AddConfiguredCacheManager(this IServiceCollection services)
    {
        return services.AddSingleton(provider
            => new IdCacheManager(
                new DataCache<ulong>().AddRangeAsync(provider
                    .GetRequiredService<MyDbContext>().Users
                    .Select(u => u.Id)
                    .ToList()).Result,
                new DataCache<ulong>().AddRangeAsync(provider
                    .GetRequiredService<MyDbContext>().Channels
                    .Select(c => c.Id)
                    .ToList()).Result,
                new DataCache<uint>().AddRangeAsync(provider
                    .GetRequiredService<MyDbContext>().BackupRegistries
                    .Select(b => b.Id)
                    .ToList()).Result
            ));
    }
}