using System.Collections.Immutable;
using Discord;
using Discord.WebSocket;
using OPZBackup.Data;
using OPZBackup.Data.Dto;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.Services.Utils;

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
        SaveMessages(batch);

        if (batch.Downloadables.Any())
            await DownloadMessageAttachments(batch.Downloadables, cancelToken);

        await _backupRepository.CommitChangesAsync();

        _logger.BatchSaved();
    }

    private async Task<IEnumerable<IMessage>> FetchMessagesAsync(ulong startAfterMessageId)
    {
        var attempts = 0;

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

                _logger.MessagesFetched();
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

        while (true)
        {
            try
            {
                var processedMessages =
                    await _messageProcessor.ProcessAsync(rawMessages, _backupContext, cancellationToken);

                _logger.MessagesProcessed();
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

        _backupRepository.SaveMessages(batch.ProcessedMessages);

        if (batch.NewUsers.Any())
            _backupRepository.SaveUsers(batch.NewUsers);

        _logger.MessagesSaved();
    }

    private async Task DownloadMessageAttachments(IEnumerable<Downloadable> toDownload, CancellationToken cancelToken)
    {

        var fileCount = 0;
        foreach (var downloadable in toDownload)
            fileCount += downloadable.Attachments.Count();

        _logger.Log.Information("Downloading {fileCount} attachments", fileCount);

        var writtenAttachments = await _attachmentDownloader
            .DownloadRangeAsync(toDownload, _backupContext, cancelToken);

        _backupRepository.SaveAttachments(writtenAttachments);

        _logger.FilesDownloaded();
    }
}