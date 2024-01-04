// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPZBot.DataAccess.Context;
using OPZBot.Extensions;
using OPZBot.Logging;
using Serilog;
using Serilog.Events;

namespace OPZBot;

public class Program
{
    public const string APP_VER = "1.0.1";
    public static DateTime SessionTime { get; } = DateTime.Now;
    public static string FileBackupPath { get; } = $"{AppContext.BaseDirectory}Backup/Files";
    public static bool RunWithCooldowns { get; private set; }
    public static int TimezoneAdjust { get; private set; }
    public static ulong? MainAdminRoleId { get; private set; }
    public static ulong BotUserId { get; private set; }

    public static Task Main(string[] args)
    {
        return new Program().MainAsync(args);
    }

    public async Task MainAsync(string[] args)
    {
        new StartupConfigMenu().Initialize();
        ConfigureStaticLogger();

        try
        {
            Log.Information($"OPZBot - v{APP_VER} \n"+ "Starting host");

            var hostBuilder = Host.CreateDefaultBuilder(args)
                .UseSerilog((_, _, cfg)
                        => cfg
                            .Enrich.FromLogContext()
                            .MinimumLevel.Information()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .WriteTo.Console(), preserveStaticLogger: true
                );

            SetStartupBotValues(hostBuilder);
            using var host = hostBuilder.Build();
            await CreateDbFileIfNotExists(host);
            await RunAsync(host);
        }
        catch (HostAbortedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            await LogFileWritter.LogHostCrash(ex);
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            Environment.ExitCode = 1;
        }
    }

    //Start running the bot asynchronously
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
        };

        await client.LoginAsync(TokenType.Bot, config["Token"]);
        await client.StartAsync();
        await Task.Delay(-1);
    }

    private Task CreateDbFileIfNotExists(IHost host)
    {
        using var serviceScope = host.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<MyDbContext>();
        return context.Database.EnsureCreatedAsync();
    }

    private void SetStartupBotValues(IHostBuilder hostBuilder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(BotConfigService.CONFIG_FILE_NAME)
            .Build();

        MainAdminRoleId = config.GetValue<ulong?>("MainAdminRoleId");
        RunWithCooldowns = config.GetValue<bool>("RunWithCooldowns");
        TimezoneAdjust = config.GetValue<int>("TimezoneAdjust");
        if (!RunWithCooldowns) Log.Warning("Running without cooldowns!");

        hostBuilder.ConfigureBotServices(config);
    }

    private void ConfigureStaticLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
    }
}