using Discord.Interactions;

namespace OPZBot.Services.MessageBackup;

public interface IResponseHandler
{
    Task SendStartNotificationAsync(object? sender, BackupEventArgs e);
    Task SendBatchFinishedAsync(object? sender, BackupEventArgs e);
    Task SendCompletedAsync(object? sender, BackupEventArgs e);
    Task SendFailedAsync(object? sender, BackupEventArgs e);
    Task SendInvalidAttemptAsync(SocketInteractionContext context, TimeSpan cooldownTime);
    Task SendDeleteConfirmationAsync(SocketInteractionContext context);
    Task SendUserDeletionResultAsync(SocketInteractionContext context, bool wasDeleted);
    Task SendEmptyBackupAsync(object? sender, BackupEventArgs args);
}