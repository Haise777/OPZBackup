using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Extensions;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.ResponseHandlers.Backup;
using OPZBackup.Services.Utils;

// ReSharper disable PossibleMultipleEnumeration TODO-4

namespace OPZBackup.Services.Backup;

public class BackupService : IAsyncDisposable
{
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly CancellationToken _cancelToken;
    private readonly CancellationTokenSource _cancelTokenSource;
    private readonly BackupContextFactory _contextFactory;
    private readonly MyDbContext _dbContext;
    private readonly DirCompressor _dirCompressor;
    private readonly FileCleaner _fileCleaner;
    private readonly BackupLogger _logger;
    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly PerformanceProfiler _profiler;
    private BackupContext _context = null!;
    private ServiceResponseHandler _responseHandler = null!;

    public BackupService(MessageFetcher messageFetcher,
        MessageProcessor messageProcessor,
        BackupContextFactory contextFactory,
        MyDbContext dbContext,
        AttachmentDownloader attachmentDownloader,
        DirCompressor dirCompressor, FileCleaner fileCleaner,
        BackupLogger logger, PerformanceProfiler profiler)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
        _dirCompressor = dirCompressor;
        _fileCleaner = fileCleaner;
        _logger = logger;
        _profiler = profiler;
        _cancelTokenSource = new CancellationTokenSource();
        _cancelToken = _cancelTokenSource.Token;
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _logger.DisposeAsync();
        await _context.DisposeAsync();
        _cancelTokenSource.Dispose();
    }

    public async Task StartBackupAsync(SocketInteractionContext interactionContext,
        ServiceResponseHandler responseHandler, bool isUntilLast)
    {
        _responseHandler = responseHandler;
        _context = await _contextFactory.RegisterNewBackup(interactionContext, isUntilLast);
        _profiler.Subscribe(nameof(SaveBatch));
        _profiler.Subscribe(nameof(DownloadMessageAttachments));
        _profiler.Subscribe(nameof(CompressFiles));

        try
        {
            await _responseHandler.SendStartNotificationAsync(_context);

            await BackupMessages();
            await CompressFiles();
            await IncrementStatistics();
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

        _logger.Log.Information("Backup finished in {time}",
            _profiler.TotalElapsed(true, nameof(SaveBatch), nameof(DownloadMessageAttachments)).Formatted());
        await _responseHandler.SendCompletedAsync(_context);
    }

    private async Task IncrementStatistics()
    {
        _logger.Log.Information("Updating statistic data.");

        var tracker = _context.StatisticTracker;

        foreach (var userStatistics in tracker.GetStatistics())
        {
            var user = await _dbContext.Users.FirstAsync(u => u.Id == userStatistics.Key);
            user.MessageCount += userStatistics.Value.MessageCount;
            user.FileCount += userStatistics.Value.FileCount;
            user.ByteSize += userStatistics.Value.ByteSize;
        }

        var channel = await _dbContext.Channels.FirstAsync(c => c.Id == _context.BackupRegistry.ChannelId);
        var total = tracker.GetTotalStatistics();
        channel.MessageCount += total.MessageCount;
        channel.FileCount += total.FileCount;
        channel.ByteSize += total.ByteSize;

        await _dbContext.SaveChangesAsync();
    }

    private async Task BackupMessages()
    {
        _logger.Log.Information("Starting backup");
        var timer = _profiler.Subscribe("batch");

        ulong lastMessageId = 0;

        while (true)
        {
            _cancelToken.ThrowIfCancellationRequested();
            timer.StartTimer();

            if (_context.IsStopped)
            {
                _logger.Log.Information("Reached last saved message, finishing backup...");
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

            timer.Stop();
            _context.MessageCount += backupBatch.Messages.Count();
            _logger.Log.Information("Batch '{n}' finished in {elapsed} | {mean}",
                ++_context.BatchNumber, timer.Elapsed.Formatted(), timer.Mean.Formatted());
            await _responseHandler.SendBatchFinishedAsync(_context, backupBatch);
        }
    }

    private async Task SaveBatch(BackupBatch batch)
    {
        var timer = _profiler.Timers[nameof(SaveBatch)];
        timer.StartTimer();
        _dbContext.Messages.AddRange(batch.Messages);

        if (batch.Users.Any())
            _dbContext.Users.AddRange(batch.Users);

        //TODO-4 Execute in parallel the db save and download
        await _dbContext.SaveChangesAsync();
        timer.Stop();
        _logger.Log.Information("Batch saved in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());

        if (batch.ToDownload.Any())
            await DownloadMessageAttachments(batch.ToDownload);
    }

    private async Task DownloadMessageAttachments(IEnumerable<Downloadable> toDownload)
    {
        var timer = _profiler.Timers[nameof(DownloadMessageAttachments)];
        timer.StartTimer();

        var fileCount = 0;
        foreach (var downloadable in toDownload)
            fileCount += downloadable.Attachments.Count();

        _logger.Log.Information("Downloading {fileCount} attachments", fileCount);
        _context.FileCount += fileCount;
        await _attachmentDownloader.DownloadRangeAsync(toDownload, _cancelToken);
        timer.Stop();
        _logger.Log.Information("Download finished in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
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

    private async Task CompressFiles()
    {
        var timer = _profiler.Timers[nameof(CompressFiles)];
        //TODO-3 Implement a way of tracking the progress of the compression
        if (_context.FileCount == 0)
            return;

        timer.StartTimer();
        _logger.Log.Information("Compressing files");
        await _responseHandler.SendCompressingFilesAsync(_context);
        await _dirCompressor.CompressAsync(
            $"{App.TempPath}/{_context.BackupRegistry.ChannelId}",
            $"{App.BackupPath}",
            _cancelToken
        );
        timer.Stop();
        _logger.Log.Information("Files compressed in {seconds}", timer.Elapsed.Formatted());
        await _fileCleaner.DeleteDirAsync(App.TempPath);
    }

    public async Task CancelAsync()
    {
        await _cancelTokenSource.CancelAsync();
    }
}