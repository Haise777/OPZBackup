// Copyright (c) 2024, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPZBackup.Logging;
using Serilog;
using Serilog.Events;

namespace OPZBackup;

public class Program : StartupBase
{
    public static Task Main(string[] args)
        => MainAsync(args);

    static async Task MainAsync(string[] args)
    {
        new StartupConfigMenu().Initialize();
        ConfigureStaticLogger();

        try
        {
            Log.Information($"OPZBot - v{AppInfo.APP_VER} \n"+ "Starting host");

            var hostBuilder = Host.CreateDefaultBuilder(args)
                .UseSerilog((_, _, cfg)
                        => cfg
                            .Enrich.FromLogContext()
                            .MinimumLevel.Information()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .WriteTo.Console(), preserveStaticLogger: true
                );
            
            if (!AppInfo.RunWithCooldowns) 
                Log.Warning("Running without cooldowns!");
            
            //TODO ConfigureServices
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
    
    private static async Task RunAsync(IHost host)
    {
        using var serviceScope = host.Services.CreateScope();
        var services = GetStartupServices(serviceScope.ServiceProvider);
        
        await services.interactionHandler.InitializeAsync();
        ConfigureApplication(services);
        
        await services.SocketClient.LoginAsync(TokenType.Bot, AppInfo.Token);
        await services.SocketClient.StartAsync();
        await Task.Delay(-1);
    }
}