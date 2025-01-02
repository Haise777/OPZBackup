using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPZBackup.Logger;
using Serilog;
using Serilog.Debugging;

namespace OPZBackup;

public class Program : StartupBase
{
    public static Task Main(string[] args)
    {
        return MainAsync(args);
    }

    private static async Task MainAsync(string[] args)
    {
        //TODO: Implement a terminal to perform some commands to the bot
        ConfigureStaticLogger();

        try
        {
            Log.Information($"OPZBot - v{App.Version} \n" + "Starting host");

            var hostBuilder = Host.CreateDefaultBuilder(args)
                .UseSerilog((_, _, cfg) => LoggerConfig.GetMainConfiguration(cfg)
                    , true
                );

            if (!App.RunWithCooldowns)
                Log.Warning("Running without cooldowns!");
            if (Dev.IsCleanRun)
            {
                Log.Warning("!! CLEAN RUN flag is set to true");
                Dev.DoCleanRun();
            }

            ConfigureServices(hostBuilder);
            using var host = hostBuilder.Build();
            SelfLog.Enable(Console.Out);

            if (!Directory.Exists(App.BackupPath))
                Directory.CreateDirectory(App.BackupPath);

            if (await CreateDbFileIfNotExists(host))
                Log.Information("Database file has been created");

            await RunAsync(host);
        }
        catch (HostAbortedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            await using var newLog = new LoggerConfiguration()
                .WriteTo.File($"logs/crash_{App.SessionDate:yyyyMMdd_HH-mm-ss}.txt").CreateLogger();

            newLog.Fatal(ex, "Host terminated unexpectedly");
            //await LogWritter.LogHostCrash(ex);
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

        await services.SocketClient.LoginAsync(TokenType.Bot, App.Token);
        await services.SocketClient.StartAsync();
        await Task.Delay(-1);
    }
}