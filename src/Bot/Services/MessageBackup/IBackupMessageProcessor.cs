using Discord;

namespace OPZBot.Services.MessageBackup;

public interface IBackupMessageProcessor
{
    public bool IsUntilLastBackup { get; set; }
    event Action? EndBackupProcess;
    Task<MessageDataBatchDto> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint backupId);
}