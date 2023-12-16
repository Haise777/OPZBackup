using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OPZBot.DataAccess.Models;
using OPZBot.Logging;

namespace OPZBot.Services.MessageBackup;

public class BackupLogging
{
    private readonly ILogger<BackupService> _logger;

    public BackupLogging(ILogger<BackupService> logger)
    {
        _logger = logger;
    }

    public Task LogBackupStart(SocketInteractionContext context, BackupRegistry registry)
        => _logger.LogAsync(LogLevel.Information, null,
            $"{nameof(BackupService)} > Started backup process with registry id: {registry.Id}");


    public Task LogBatchFinished(SocketInteractionContext context, MessageDataBatchDto batch, BackupRegistry registry)
        => _logger.LogAsync(LogLevel.Information, null,
            $"{nameof(BackupService)} > {registry.Id} >" +
            $" Finished batch with {batch.Messages.Count()} new messages /" +
            $" {batch.Messages.Count()} new users");


    public Task LogBackupCompleted(SocketInteractionContext context, BackupRegistry registry)
        => _logger.LogAsync(LogLevel.Information, null,
            $"{nameof(BackupService)} > {registry.Id} > Backup process completed");
}