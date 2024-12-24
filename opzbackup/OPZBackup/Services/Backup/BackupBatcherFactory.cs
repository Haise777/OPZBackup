using Discord.WebSocket;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services.Backup;


public class BackupBatcherFactory
{

    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;

    public BackupBatcherFactory(MessageFetcher messageFetcher, MessageProcessor messageProcessor)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
    }

    public BackupBatch Create(BackupContext backupContext, ISocketMessageChannel socketMessageChannel)
    {
        return new BackupBatch(_messageFetcher, _messageProcessor, backupContext, socketMessageChannel);
    }

}