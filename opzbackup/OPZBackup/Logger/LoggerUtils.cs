using AnsiStyles;
using Discord;
using Serilog.Events;

namespace OPZBackup.Logger;

public static class LoggerUtils
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
    
    public static string ColorText(string text, ushort color)
    {
        var colorCode = StringStyle.Foreground[color];
        var resetCode = StringStyle.Reset;

        return $"{colorCode}{text}{resetCode}";
    }
}