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

    public BackupBatcher Create(BackupContext backupContext, ISocketMessageChannel socketMessageChannel)
    {
        return new BackupBatcher(_messageFetcher, _messageProcessor, backupContext, socketMessageChannel);
    }

}