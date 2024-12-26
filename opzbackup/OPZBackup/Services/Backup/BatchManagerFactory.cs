using Discord.WebSocket;
using OPZBackup.Data;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.Services.Utils;
using Timer = OPZBackup.Services.Utils.Timer;

namespace OPZBackup.Services.Backup;


public class BatchManagerFactory
{

    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly BackupLogger _backupLogger;
    private readonly MyDbContext _dbContext;

    public BatchManagerFactory(MessageFetcher messageFetcher, MessageProcessor messageProcessor, AttachmentDownloader attachmentDownloader, BackupLogger backupLogger, MyDbContext dbContext)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _attachmentDownloader = attachmentDownloader;
        _backupLogger = backupLogger;
        _dbContext = dbContext;
    }

    public BatchManager Create(BackupContext backupContext, ISocketMessageChannel socketMessageChannel, Timer saveTimer, Timer downloadTimer)
    {
        return new BatchManager(
                _messageFetcher,
                _messageProcessor,
                _dbContext,
                _backupLogger,
                _attachmentDownloader,
                socketMessageChannel,
                backupContext,
                saveTimer,
                downloadTimer
            );
    }

}