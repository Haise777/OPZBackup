using Discord;
using Discord.Interactions;
using OPZBackup.Data;
using OPZBackup.Exceptions;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.ResponseHandlers.Backup;
using OPZBackup.Services.Utils;
using Serilog;

// ReSharper disable PossibleMultipleEnumeration TODO-4
//TODO-3 Better organize the logfiles in a log folder and have specific behavior for different types of logs, like append, new file, etc
//TODO-Feature Implement passive benchmarking capabilities to various bot functions, time per batch, average per batch, response time, etc


namespace OPZBackup.Services.Backup;

public class BackupService : IAsyncDisposable
{
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly BackupContextFactory _contextFactory;
    private readonly MyDbContext _dbContext;
    private readonly DirCompressor _dirCompressor;
    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly BackupLogger _logger;
    private BackupContext _context = null!;
    private ServiceResponseHandler _responseHandler = null!;
    private readonly FileCleaner _fileCleaner;
    private readonly CancellationTokenSource _cancelTokenSource;
    private readonly CancellationToken _cancelToken;

    public BackupService(MessageFetcher messageFetcher,
        MessageProcessor messageProcessor,
        BackupContextFactory contextFactory,
        MyDbContext dbContext,
        AttachmentDownloader attachmentDownloader,
        DirCompressor dirCompressor, FileCleaner fileCleaner,
        BackupLogger logger)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
        _dirCompressor = dirCompressor;
        _fileCleaner = fileCleaner;
        _logger = logger;
        _cancelTokenSource = new CancellationTokenSource();
        _cancelToken = _cancelTokenSource.Token;
    }

    public async Task StartBackupAsync(SocketInteractionContext interactionContext,
        ServiceResponseHandler responseHandler, bool isUntilLast)
    {
        _responseHandler = responseHandler;
        _context = await _contextFactory.RegisterNewBackup(interactionContext, isUntilLast);

        try
        {
            await _responseHandler.SendStartNotificationAsync(_context);

            await BackupMessages();
            await CompressFilesAsync();
        }
        catch (OperationCanceledException)
        {
            //TODO-2 Log cancellation
            _logger.Log.Information("Backup canceled.");
            var sendCancelled = _responseHandler.SendProcessCancelledAsync();
            var rollBack = _context.RollbackAsync();
            await rollBack;
            await sendCancelled;
            return;
        }
        catch (Exception e)
        {
            //TODO-2 Log exception and also send the error name back to the client
            _logger.Log.Error(e, "Backup failed.");
            var sendFailed = _responseHandler.SendFailedAsync(_context);
            var rollBack = _context.RollbackAsync();
            await rollBack;
            await sendFailed;

            throw;
        }

        _logger.Log.Information("Backup finished");
        await _responseHandler.SendCompletedAsync(_context);
    }

    private async Task BackupMessages()
    {
        _logger.Log.Information("Starting backup");

        ulong lastMessageId = 0;

        while (true)
        {
            _cancelToken.ThrowIfCancellationRequested();

            if (_context.IsStopped)
            {
                _logger.Log.Information("Reached already saved message, finishing backup...");
                break;
            }

            var fetchedMessages = await FetchMessages(lastMessageId);
            if (!fetchedMessages.Any())
            {
                _logger.Log.Information("Reached the end of the channel, finishing backup...");
                break;
            }

            lastMessageId = fetchedMessages.Last().Id;

            var backupBatch = await _messageProcessor.ProcessAsync(fetchedMessages, _context, _cancelToken);
            if (!backupBatch.Messages.Any())
            {
                _logger.Log.Information("No messages in current batch, skipping...");
                continue;
            }

            await SaveBatch(backupBatch);
            _context.MessageCount += backupBatch.Messages.Count();
            _logger.Log.Information("Batch '{n}' finished", ++_context.BatchNumber);
            await _responseHandler.SendBatchFinishedAsync(_context, backupBatch);
        }
    }

    private async Task SaveBatch(BackupBatch batch)
    {
        _dbContext.Messages.AddRange(batch.Messages);

        if (batch.Users.Any())
            _dbContext.Users.AddRange(batch.Users);

        //TODO-4 Execute in parallel the db save and download
        await _dbContext.SaveChangesAsync();

        if (batch.ToDownload.Any())
            await DownloadMessageAttachments(batch.ToDownload);
    }

    private async Task DownloadMessageAttachments(IEnumerable<Downloadable> toDownload)
    {
        var fileCount = 0;
        foreach (var downloadable in toDownload)
            fileCount += downloadable.Attachments.Count();

        _logger.Log.Information("Downloading {fileCount} attachments", fileCount);
        _context.FileCount += fileCount;
        await _attachmentDownloader.DownloadRangeAsync(toDownload, _cancelToken);
    }

    private async Task<IEnumerable<IMessage>> FetchMessages(ulong lastMessageId)
    {
        var channelContext = _context.InteractionContext.Channel;
        _logger.Log.Information("Fetching messages...");

        return lastMessageId switch
        {
            0 => await _messageFetcher.FetchAsync(channelContext),
            _ => await _messageFetcher.FetchAsync(channelContext, lastMessageId)
        };
    }

    private async Task CompressFilesAsync()
    {
        //TODO-3 Implement a way of tracking the progress of the compression
        if (_context.FileCount == 0)
            return;

        _logger.Log.Information("Compressing files");
        await _responseHandler.SendCompressingFilesAsync(_context);
        await _dirCompressor.CompressAsync(
            $"{App.TempPath}/{_context.BackupRegistry.ChannelId}",
            $"{App.BackupPath}",
            _cancelToken
        );
        _logger.Log.Information("Files compressed");
        await _fileCleaner.DeleteDirAsync(App.TempPath);
    }

    public async Task CancelAsync()
    {
        await _cancelTokenSource.CancelAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _logger.DisposeAsync();
        await _context.DisposeAsync();
        _cancelTokenSource.Dispose();
    }
}