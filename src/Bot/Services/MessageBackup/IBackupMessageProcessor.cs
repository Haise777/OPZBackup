using Discord;

namespace OPZBot.Services.MessageBackup;

public interface IBackupMessageProcessor
{
    public bool IsUntilLastBackup { get; set; }
    event Action? FinishBackupProcess;
    Task<ProcessedMessageData> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint backupId);
}