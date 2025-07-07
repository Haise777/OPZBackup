using System.Collections.Immutable;
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
    // Se livrar desse monte de logica de timer, é pra ir tudo pro BackupContext.Performance...
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly BackupContext _backupContext;
    private readonly BackupLogger _logger;
    private readonly IBackupRepository _backupRepository;

    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly ISocketMessageChannel _socketMessageChannel;

    private Timer SaveTimer => _backupContext.PerformanceProfiler.SaveTimer;
    private Timer ProcessTimer => _backupContext.PerformanceProfiler.ProcessTimer;
    private Timer SaveMessagesTimer => _backupContext.PerformanceProfiler.SaveMessagesTimer;
    private Timer FetchTimer => _backupContext.PerformanceProfiler.FetchTimer;
    private Timer DownloadTimer => _backupContext.PerformanceProfiler.DownloadTimer;

    public int BatchNumber { get; set; }

    public BatchManager(MessageFetcher messageFetcher, MessageProcessor messageProcessor,
        BackupLogger logger, AttachmentDownloader attachmentDownloader, ISocketMessageChannel socketChannel,
        BackupContext backupContext, IBackupRepository backupRepository)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _logger = logger;
        _attachmentDownloader = attachmentDownloader;
        _backupContext = backupContext;
        _socketMessageChannel = socketChannel;
        _backupRepository = backupRepository;
    }

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

    //TODO: Separate a proper repository to abstract away this data methods
    public async Task SaveBatchAsync(BackupBatch batch, CancellationToken cancelToken)
    {
        SaveTimer.StartTimer();
        SaveMessages(batch);

        if (batch.Downloadables.Any())
            await DownloadMessageAttachments(batch.Downloadables, cancelToken);

        await _backupRepository.CommitChangesAsync();

        _logger.BatchSaved(SaveTimer.Stop());
    }

    private async Task<IEnumerable<IMessage>> FetchMessagesAsync(ulong startAfterMessageId)
    {
        var attempts = 0;
        FetchTimer.StartTimer();

        while (true)
        {
            try
            {
                _logger.Log.Information("Fetching messages...");

                var fetchedMessages = startAfterMessageId switch
                {
                    0 => await _messageFetcher.FetchAsync(_socketMessageChannel),
                    _ => await _messageFetcher.FetchAsync(_socketMessageChannel, startAfterMessageId)
                };

                _logger.MessagesFetched(FetchTimer.Stop());
                return fetchedMessages;
            }
            catch (Exception e)
            {
                //Log here

                if (++attempts > 3)
                    throw;
            }
        }
    }

    private async Task<ProcessedBatch> ProcessAsync(IEnumerable<IMessage> rawMessages,
        CancellationToken cancellationToken)
    {
        var attempts = 0;
        ProcessTimer.StartTimer();

        while (true)
        {
            try
            {
                ProcessTimer.StartTimer();
                var processedMessages =
                    await _messageProcessor.ProcessAsync(rawMessages, _backupContext, cancellationToken);

                _logger.MessagesProcessed(ProcessTimer.Stop());
                return processedMessages;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Log here

                if (++attempts > 3)
                    throw;
            }
        }
    }

    private void SaveMessages(BackupBatch batch)
    {
        SaveMessagesTimer.StartTimer();

        _backupRepository.SaveMessages(batch.ProcessedMessages);

        if (batch.NewUsers.Any())
            _backupRepository.SaveUsers(batch.NewUsers);

        _logger.MessagesSaved(SaveMessagesTimer.Stop());
    }

    private async Task DownloadMessageAttachments(IEnumerable<Downloadable> toDownload, CancellationToken cancelToken)
    {
        DownloadTimer.StartTimer();

        var fileCount = 0;
        foreach (var downloadable in toDownload)
            fileCount += downloadable.Attachments.Count();

        _logger.Log.Information("Downloading {fileCount} attachments", fileCount);

        var writtenAttachments = await _attachmentDownloader
            .DownloadRangeAsync(toDownload, _backupContext, cancelToken);

        _backupRepository.SaveAttachments(writtenAttachments);

        _logger.FilesDownloaded(DownloadTimer.Stop());
    }
}