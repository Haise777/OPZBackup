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
    private const string FetchTimerId = "fetch-timer";
    private const string ProcessTimerId = "process-timer";
    private const string SaveTimerId = "save-timer";
    private const string DownloadTimerId = "download-timer";
    private const string SaveMessagesId = "messages-timer";
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly BackupContext _backupContext;
    private readonly MyDbContext _dbContext;
    private readonly BackupLogger _logger;

    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly PerformanceProfiler _performanceProfiler;
    private readonly ISocketMessageChannel _socketMessageChannel;

    public BatchManager(MessageFetcher messageFetcher, MessageProcessor messageProcessor, MyDbContext dbContext,
        BackupLogger logger, AttachmentDownloader attachmentDownloader, ISocketMessageChannel socketChannel,
        BackupContext backupContext, PerformanceProfiler performanceProfiler)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _dbContext = dbContext;
        _logger = logger;
        _attachmentDownloader = attachmentDownloader;
        _backupContext = backupContext;
        _performanceProfiler = performanceProfiler;
        _socketMessageChannel = socketChannel;

        _performanceProfiler.Subscribe(FetchTimerId);
        _performanceProfiler.Subscribe(ProcessTimerId);
        _performanceProfiler.Subscribe(SaveTimerId);
        _performanceProfiler.Subscribe(DownloadTimerId);
        _performanceProfiler.Subscribe(SaveMessagesId);
    }

    public int BatchNumber { get; set; }
    public Timer FetchTimer => _performanceProfiler.Timers[FetchTimerId];
    public Timer ProcessTimer => _performanceProfiler.Timers[ProcessTimerId];
    public Timer SaveTimer => _performanceProfiler.Timers[SaveTimerId];
    public Timer DownloadTimer => _performanceProfiler.Timers[DownloadTimerId];
    public Timer SaveMessagesTimer => _performanceProfiler.Timers[SaveMessagesId];
    public TimeSpan TotalElapsed => _performanceProfiler.TotalElapsed();


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
        SaveTimer.StartTimer();
        await SaveMessages(batch);

        if (batch.Downloadables.Any())
            await DownloadMessageAttachments(batch.Downloadables, cancelToken);

        _logger.BatchSaved(SaveTimer.Stop());
    }

    private async Task SaveMessages(BackupBatch batch)
    {
        SaveMessagesTimer.StartTimer();

        _dbContext.Messages.AddRange(batch.ProcessedMessages);

        if (batch.NewUsers.Any())
            _dbContext.Users.AddRange(batch.NewUsers);

        await _dbContext.SaveChangesAsync();

        _logger.MessagesSaved(SaveMessagesTimer.Stop());
    }

    private async Task<IEnumerable<IMessage>> FetchMessagesAsync(ulong startAfterMessageId)
    {
        _logger.Log.Information("Fetching messages...");
        FetchTimer.StartTimer();

        var fetchedMessages = startAfterMessageId switch
        {
            0 => await _messageFetcher.FetchAsync(_socketMessageChannel),
            _ => await _messageFetcher.FetchAsync(_socketMessageChannel, startAfterMessageId)
        };

        _logger.MessagesFetched(FetchTimer.Stop());
        return fetchedMessages;
    }

    private async Task<ProcessedBatch> ProcessAsync(IEnumerable<IMessage> rawMessages,
        CancellationToken cancellationToken)
    {
        ProcessTimer.StartTimer();
        var processedMessages = await _messageProcessor.ProcessAsync(rawMessages, _backupContext, cancellationToken);

        _logger.MessagesProcessed(ProcessTimer.Stop());
        return processedMessages;
    }

    private async Task DownloadMessageAttachments(IEnumerable<Downloadable> toDownload, CancellationToken cancelToken)
    {
        DownloadTimer.StartTimer();

        var fileCount = 0;
        foreach (var downloadable in toDownload)
            fileCount += downloadable.Attachments.Count();

        _logger.Log.Information("Downloading {fileCount} attachments", fileCount);

        await _attachmentDownloader.DownloadRangeAsync(toDownload, _backupContext, cancelToken);
        _logger.FilesDownloaded(DownloadTimer.Stop());
    }
}