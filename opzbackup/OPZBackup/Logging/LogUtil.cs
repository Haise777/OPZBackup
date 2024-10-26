// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Microsoft.Extensions.Logging;

namespace OPZBackup.Logging;

public static class LogUtil
{
    //Parse Discord.Net LogSeverity to Microsoft.Logging's LogLevel enum
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

    //Standard logging with rich error logging capabilities
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

    //Error logging with log-to-file capabilities
    public static async Task RichLogErrorAsync<T>(this ILogger<T> logger, Exception ex, string? message,
        params object?[] args)
    {
        logger.LogError(ex, message, args);
        await LogFileWritter.LogError(ex, message);
    }

    //Log wrapper to log command executions
    public static void LogCommandExecution<T>(
        this ILogger<T> logger, string service, string author, string channel, string command, string commandArg = "")
    {
        logger.LogInformation(
            "{service}: {author} > {channel} > {command} {commandArg}", 
            service, author, channel, command, commandArg);
    }

    //Async log wrapper
    public static Task LogAsync<T>
        (this ILogger<T> logger, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
    {
        return Task.Run(() => logger.Log(logLevel, exception, message, args));
    }
}