using Discord.WebSocket;
using OPZBackup.Data;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services.Backup;

public class BatchManagerFactory
{
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly MyDbContext _dbContext;

    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly PerformanceProfiler _performanceProfiler;

    public BatchManagerFactory(MessageFetcher messageFetcher, MessageProcessor messageProcessor,
        AttachmentDownloader attachmentDownloader, MyDbContext dbContext,
        PerformanceProfiler performanceProfiler)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _attachmentDownloader = attachmentDownloader;
        _dbContext = dbContext;
        _performanceProfiler = performanceProfiler;
    }

    public BatchManager Create(BackupContext backupContext, ISocketMessageChannel socketMessageChannel, BackupLogger backupLogger)
    {
        return new BatchManager(
            _messageFetcher,
            _messageProcessor,
            _dbContext,
            backupLogger,
            _attachmentDownloader,
            socketMessageChannel,
            backupContext,
            _performanceProfiler
        );
    }
}