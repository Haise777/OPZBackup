using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace OPZBackup.Logger;

public static class EnhancedLogger
{
    //Parse Discord.Net LogSeverity to Microsoft.Logging's LogLevel
    public static LogEventLevel ParseLogLevel(LogSeverity logSeverity)
    {
        return logSeverity switch
        {
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Critical => LogEventLevel.Fatal,
            _ => throw new InvalidOperationException($"Unable to parse argument '{logSeverity}' to target value")
        };
    }
}