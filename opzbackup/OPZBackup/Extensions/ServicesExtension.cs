// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

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
using OPZBackup.Data;
using OPZBackup.Data.Caching;
using OPZBackup.Data.Context;
using OPZBackup.Services;
using OPZBackup.Services.Blacklist;
using OPZBackup.Services.MessageBackup;
using OPZBackup.Services.MessageBackup.FileBackup;
using RunMode = Discord.Interactions.RunMode;

namespace OPZBackup.Extensions;

public static class ServicesExtension
{
    public static IHostBuilder ConfigureBotServices(this IHostBuilder host, IConfigurationRoot config)
    {
        host.ConfigureServices((ctx, services) => services
            .AddDbContext<MyDbContext>(options
                => options.UseSqlite(@$"Data Source={AppContext.BaseDirectory}Backup/discord_backup.db"))
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
            .AddSingleton<IdCacheManager>()
            .AddSingleton<Mapper>()
            .AddSingleton<LoggingWrapper>()
            .AddSingleton<FileCleaner>()
            .AddSingleton<IBlacklistResponseHandler, BlacklistResponseHandler>()
            .AddScoped<IMessageFetcher, MessageFetcher>()
            .AddScoped<IBackupMessageProcessor, MessageProcessor>()
            .AddScoped<IMessageBackupService, MessageBackupService>()
            .AddScoped<IBackupResponseHandler, BackupResponseHandler>()
            .AddScoped<IFileBackupService, FileBackupService>()
            .AddScoped<IBlacklistService, BlacklistService>()
            .AddScoped<ResponseBuilder>()
            .AddHttpClient()
            .RemoveAll<IHttpMessageHandlerBuilderFilter>()
        );

        return host;
    }
}