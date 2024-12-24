using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.Extensions;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.ResponseHandlers.Backup;
using OPZBackup.Services.Utils;
using Timer = OPZBackup.Services.Utils.Timer;

// ReSharper disable PossibleMultipleEnumeration TODO

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
    private readonly PerformanceProfiler _profiler;
    private readonly BackupBatcherFactory _batcherFactory;
    private BackupContext _context = null!;
    private ServiceResponseHandler _responseHandler = null!;

    public BackupService(
        BackupContextFactory contextFactory,
        MyDbContext dbContext,
        AttachmentDownloader attachmentDownloader,
        DirCompressor dirCompressor, FileCleaner fileCleaner,
        BackupLogger logger, PerformanceProfiler profiler, BackupBatcherFactory backupBatcherFactory)
    {
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
        _dirCompressor = dirCompressor;
        _fileCleaner = fileCleaner;
        _logger = logger;
        _profiler = profiler;
        _cancelTokenSource = new CancellationTokenSource();
        _cancelToken = _cancelTokenSource.Token;
        _batcherFactory = backupBatcherFactory;
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
        _profiler.Subscribe(nameof(SaveCurrentBatch));
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
            //TODO: Log cancellation
            _logger.Log.Information("Backup canceled.");
            var sendCancelled = _responseHandler.SendProcessCancelledAsync();
            var rollBack = _context.RollbackAsync();
            await rollBack;
            await sendCancelled;
            return;
        }
        catch (Exception e)
        {
            //TODO: Log exception and also send the error name back to the client
            _logger.Log.Error(e, "Backup failed.");
            var sendFailed = _responseHandler.SendFailedAsync(_context);
            var rollBack = _context.RollbackAsync();
            await rollBack;
            await sendFailed;

            throw;
        }

        _logger.Log.Information("Backup {id} finished in {time}\n" +
                                " | Occupying {compressedTotal} in saved attachments",
            _context.BackupRegistry.Id,
            _profiler.TotalElapsed(true, nameof(SaveCurrentBatch), nameof(DownloadMessageAttachments)).Formatted(),
            ByteSizeConversor.ToFormattedString(_context.StatisticTracker.CompressedFilesSize)
        );

        await _responseHandler.SendCompletedAsync(_context, _context.BackupRegistry.Channel);
    }

    public async Task CancelAsync()
    {
        await _cancelTokenSource.CancelAsync();
    }

    private async Task BackupMessages()
    {
        _logger.Log.Information("Starting backup");
        var timer = _profiler.Subscribe("batch");
        var channelContext = _context.InteractionContext.Channel;

        ulong lastMessageId = 0;
        var attemptNumber = 0;

        while (true)
        {
            try
            {
                _cancelToken.ThrowIfCancellationRequested();
                timer.StartTimer();

                if (_context.IsStopped)
                {
                    _logger.Log.Information("Reached last saved message, finishing backup...");
                    break;
                }

                var batch = _batcherFactory.Create(_context, channelContext);
                await batch.StartBatchingAsync(lastMessageId, _cancelToken);

                if (!batch.RawMessages.Any())
                {
                    _logger.Log.Information("Reached the end of the channel, finishing backup...");
                    break;
                }
                if (!batch.ProcessedMessages.Any())
                {
                    _logger.Log.Information("No messages in current batch, skipping...");
                    continue;
                }

                await SaveCurrentBatch(batch);
                await FinishBatch(timer, batch);

                lastMessageId = batch.RawMessages.Last().Id;
                attemptNumber = 0;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                if (++attemptNumber > 3)
                    throw;

                _logger.Log.Error("Batch {batchNumber} failed, attempting again...", _context.BatchNumber);
            }
        }
    }

    private async Task SaveCurrentBatch(BackupBatcher batch)
    {
        //TODO: Implement a transaction here
        //TODO: Execute in parallel the db save and download
        var messageTimer = _profiler.Timers[nameof(SaveCurrentBatch)].StartTimer();

        _dbContext.Messages.AddRange(batch.ProcessedMessages);

        if (batch.NewUsers.Any())
            _dbContext.Users.AddRange(batch.NewUsers);

        await _dbContext.SaveChangesAsync();
        _logger.BatchSaved(messageTimer.Stop());

        if (batch.Attachments.Any())
        {
            var downloadTimer = _profiler.Timers[nameof(DownloadMessageAttachments)].StartTimer();
            await DownloadMessageAttachments(batch.Attachments);
            _logger.FilesDownloaded(downloadTimer.Stop());
        }
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

    private async Task FinishBatch(Timer timer, BackupBatcher batch)
    {
        timer.Stop();
        _context.MessageCount += batch.ProcessedMessages.Count();
        _logger.BatchFinished(timer, ++_context.BatchNumber);
        await _responseHandler.SendBatchFinishedAsync(_context, batch, timer.Mean);
    }

    private async Task CompressFiles()
    {
        var timer = _profiler.Timers[nameof(CompressFiles)];
        //TODO: Implement a way of tracking the progress of the compression
        if (_context.FileCount == 0)
            return;

        timer.StartTimer();
        _logger.Log.Information("Compressing files");
        await _responseHandler.SendCompressingFilesAsync(_context);
        var compressedSize = await _dirCompressor.CompressAsync(
            $"{App.TempPath}/{_context.BackupRegistry.ChannelId}",
            $"{App.BackupPath}",
            _cancelToken
        );
        timer.Stop();
        _logger.Log.Information("Files compressed in {seconds}", timer.Elapsed.Formatted());
        _context.StatisticTracker.CompressedFilesSize += (ulong)compressedSize;

        await _fileCleaner.DeleteDirAsync(App.TempPath);
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
        channel.CompressedByteSize += tracker.CompressedFilesSize;

        await _dbContext.SaveChangesAsync();
    }
}