using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OPZBot.DataAccess.Models;
using OPZBot.Logging;

namespace OPZBot.Services.MessageBackup;

public class LoggingWrapper
{
    private readonly ILogger<BackupService> _logger;

    public LoggingWrapper(ILogger<BackupService> logger)
    {
        _logger = logger;
    }

    public Task LogStart(object? sender, BackupEventArgs e)
    {
        return _logger.LogAsync(LogLevel.Information, null,
            "{service}: Backup {registryId} > Started backup process", 
            nameof(BackupService),
            e.Registry.Id);
    }

    public Task LogBatchFinished(object? sender, BackupEventArgs e)
    {
        return _logger.LogAsync(LogLevel.Information, null,
            "{service}: Backup {registryId} > Finished batch with {messageCount} new messages | {userCount} new users",
            nameof(BackupService),
            e.Registry.Id,
            e.MessageBatch.Messages.Count(),
            e.MessageBatch.Users.Count());
    }

    public Task LogCompleted(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        
        return _logger.LogAsync(LogLevel.Information, null,
            "{service}: Backup {registryId} > completed at batch number {batchNumber} with {savedNumber} saved messages",
            nameof(BackupService),
            e.Registry.Id,
            backupService.BatchNumber,
            backupService.SavedMessagesCount);
    }
}