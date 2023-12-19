using Discord.Interactions;
using OPZBot.Utilities;

namespace OPZBot.Services.MessageBackup;

public interface IBackupMessageService : IBackupService
{
    int BatchNumber { get; }
    int SavedMessagesCount { get; }
    int SavedFilesCount { get; }
    event AsyncEventHandler<BackupEventArgs>? StartedBackupProcess;
    event AsyncEventHandler<BackupEventArgs>? FinishedBatch;
    event AsyncEventHandler<BackupEventArgs>? CompletedBackupProcess;
    event AsyncEventHandler<BackupEventArgs>? ProcessHasFailed;
    event AsyncEventHandler<BackupEventArgs>? EmptyBackupAttempt;
    Task StartBackupAsync(SocketInteractionContext context, bool isUntilLastBackup);
}