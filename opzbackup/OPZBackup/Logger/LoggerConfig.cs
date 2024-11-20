using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace OPZBackup.Logger;

public static class LoggerConfig
{
    private const string _fileDateFormat = "yyyyMMdd_HH-mm-ss";
    public const string LogFilePath = "logs";

    public static LoggerConfiguration GetMainConfiguration(LoggerConfiguration configuration)
    {
        return configuration.WriteTo.Logger(l => l //TODO-3 Place this elsewhere
            .Filter.ByIncludingOnly(Matching.WithProperty("System"))
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Async(f => f.File($"{LogFilePath}/session/session_{App.SessionDate.ToString(_fileDateFormat)}.txt",
                outputTemplate: OutputTemplate.SplitDefaultTemplate("System")))
            .WriteTo.Console(outputTemplate: OutputTemplate.DefaultTemplate("System"))
        );
    }
}