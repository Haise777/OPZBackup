using Discord;
using Discord.WebSocket;
using OPZBackup.Data;
using OPZBackup.Data.Dto;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.Services.Utils;
using Timer = OPZBackup.Services.Utils.Timer;

namespace OPZBackup.Services.Backup;


public class BatchManager
{

    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly BackupContext _backupContext;
    private readonly ISocketMessageChannel _socketMessageChannel;
    private readonly MyDbContext _dbContext;
    private readonly BackupLogger _logger;
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly Timer _saveTimer;
    private readonly Timer _downloadTimer;
    public int BatchNumber { get; set; }

    public BatchManager(MessageFetcher messageFetcher, MessageProcessor messageProcessor, MyDbContext dbContext, BackupLogger logger, AttachmentDownloader attachmentDownloader, ISocketMessageChannel socketChannel, BackupContext backupContext, Timer saveTimer, Timer downloadTimer)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _dbContext = dbContext;
        _logger = logger;
        _attachmentDownloader = attachmentDownloader;
        _backupContext = backupContext;
        _socketMessageChannel = socketChannel;
        _saveTimer = saveTimer;
        _downloadTimer = downloadTimer;
    }


    //TODO: Implement retries in each main step of this process
    public async Task<BackupBatch> StartBatchingAsync(ulong startAfterMessageId, CancellationToken cancellationToken)
    {
        var rawMessages = await FetchMessagesAsync(startAfterMessageId);

        if (!rawMessages.Any())
            return new BackupBatch(BatchNumber, [], [], [], []);

        var processedBatch = await ProcessAsync(rawMessages, cancellationToken);

        BatchNumber++;
        return new BackupBatch(
            BatchNumber,
            rawMessages,
            processedBatch.Messages,
            processedBatch.ToDownload,
            processedBatch.Users
        );
    }

    //TODO: Implement a transaction here
    //TODO: Execute in parallel the db save and download
    public async Task SaveBatchAsync(BackupBatch batch, CancellationToken cancelToken) 
    {
        _saveTimer.StartTimer();

        _dbContext.Messages.AddRange(batch.ProcessedMessages);
        if (batch.NewUsers.Any())
        {
            _dbContext.Users.AddRange(batch.NewUsers);
        }

        await _dbContext.SaveChangesAsync();
        _logger.BatchSaved(_saveTimer.Stop());

        if (batch.Downloadables.Any())
        {
            _downloadTimer.StartTimer();
            await DownloadMessageAttachments(batch.Downloadables, cancelToken);
            _logger.FilesDownloaded(_downloadTimer.Stop());
        }
    }

    private async Task<IEnumerable<IMessage>> FetchMessagesAsync(ulong startAfterMessageId)
    {
        _logger.Log.Information("Fetching messages...");

        return startAfterMessageId switch
        {
            0 => await _messageFetcher.FetchAsync(_socketMessageChannel),
            _ => await _messageFetcher.FetchAsync(_socketMessageChannel, startAfterMessageId)
        };
    }

    private async Task<ProcessedBatch> ProcessAsync(IEnumerable<IMessage> rawMessages,CancellationToken cancellationToken)
    {
        return await _messageProcessor.ProcessAsync(rawMessages, _backupContext, cancellationToken);
    }

    private async Task DownloadMessageAttachments(IEnumerable<Downloadable> toDownload, CancellationToken cancelToken)
    {
        var fileCount = 0;
        foreach (var downloadable in toDownload)
            fileCount += downloadable.Attachments.Count();

        _logger.Log.Information("Downloading {fileCount} attachments", fileCount);
        _backupContext.FileCount += fileCount;

        await _attachmentDownloader.DownloadRangeAsync(toDownload, cancelToken);
    }
}