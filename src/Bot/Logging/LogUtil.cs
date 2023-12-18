using Discord;
using Microsoft.Extensions.Logging;

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

    public static async Task RichLogAsync<T>
        (this ILogger<T> logger, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
    {
        if (logLevel is LogLevel.Error or LogLevel.Critical && exception is not null)
        {
            await logger.RichLogErrorAsync(exception, message, args);
            return;
        }

        logger.Log(logLevel, exception, message, args);
    }

    public static async Task RichLogErrorAsync<T>(this ILogger<T> logger, Exception ex, string? message, params object?[] args)
    {
        if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "errorlogs")))
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "errorlogs"));

        var sessionDate = $"{Program.SessionDate:dd.MM.yyyy_HH.mm.ss}";
        await using var fileWriter = new StreamWriter(
            Path.Combine(AppContext.BaseDirectory, $"errorlogs\\log_{sessionDate}.log"), true);
        
        logger.LogError(ex, message, args);
        await fileWriter.WriteLineAsync($"{DateTime.Now}\n{message}\n{ex}\n\n");
    }

    public static void LogCommandExecution<T>
        (this ILogger<T> logger, string service, string author, string channel, string command, string commandArg = "")
    {
        logger.LogInformation(
            "{service}: {author} > {channel} > {command} {commandArg}", service, author, channel, command, commandArg);
    }

    public static Task LogAsync<T>
        (this ILogger<T> logger, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
    {
        return Task.Run(() => logger.Log(logLevel,exception, message, args));
    }
}