using Discord;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace OPZBot.Logging;

public static class LogUtil
{
    public static LogLevel ParseLogLevel(LogSeverity logSeverity)
    {
        return logSeverity switch
        {
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Critical => LogLevel.Critical,
            _ => throw new InvalidOperationException($"Unable to parse argument '{logSeverity}' to target value")
        };
    }

    public static async Task RichLogAsync<T>(this ILogger<T> logger, LogLevel logLevel, Exception? exception,
        string message)
    {
        if (logLevel is LogLevel.Error or LogLevel.Critical && exception is not null)
        {
            await logger.RichLogErrorAsync(exception, message);
            return;
        }

        logger.Log(logLevel, exception, message);
    }

    public static async Task RichLogErrorAsync<T>(this ILogger<T> logger, Exception ex, string? message = null)
    {
        if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "errorlogs")))
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "errorlogs"));

        var sessionDate = $"{Program.SessionDate:dd.MM.yyyy_H.mm.ss}";
        await using var fileWriter = new StreamWriter(
            Path.Combine(AppContext.BaseDirectory, $"errorlogs\\log_{sessionDate}"), true);

        logger.LogError(ex, message);
        await fileWriter.WriteLineAsync($"{DateTime.Now}\n{message}\n{ex}\n\n");
    }

    public static void LogCommandExecution<T>(this ILogger<T> logger, string service, string author, string channel,
        string command)
        => logger.LogInformation(
            "{service}: {author} > {channel} > {command}", service, author, channel, command);

    public static Task LogAsync<T>(this ILogger<T> l, LogLevel logLevel, Exception? exception, string message)
        => Task.Run(() => l.Log(logLevel, exception, message));
}