using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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

public class BackupProcess : IAsyncDisposable
{
    private const string TimerSaveMessageId = "save-messages";
    private const string TimerDownloadId = "download";

    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly CancellationToken _cancelToken;
    private readonly CancellationTokenSource _cancelTokenSource;
    private readonly BackupContextFactory _contextFactory;
    private readonly MyDbContext _dbContext;
    private readonly DirCompressor _dirCompressor;
    private readonly FileCleaner _fileCleaner;
    private readonly BackupLogger _logger;
    private readonly PerformanceProfiler _profiler;
    private readonly BatchManagerFactory _batchManagerFactory;
    private readonly Mapper _mapper;
    private readonly BackupCompressor _backupCompressor;
    private BatchManager _batchManager = null!;
    private BackupContext _context = null!;
    private ServiceResponseHandler _responseHandler = null!;

    #region constructor
    public BackupProcess(
        BackupContextFactory contextFactory,
        MyDbContext dbContext,
        AttachmentDownloader attachmentDownloader,
        DirCompressor dirCompressor, FileCleaner fileCleaner,
        BackupLogger logger, PerformanceProfiler profiler,
        BatchManagerFactory batchManagerFactory,
        Mapper mapper,
        BackupCompressor backupCompressor)
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
        _batchManagerFactory = batchManagerFactory;
        _mapper = mapper;
        _backupCompressor = backupCompressor;
    }
    #endregion

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _logger.DisposeAsync();
        _cancelTokenSource.Dispose();
    }

    public async Task CancelAsync()
    {
        await _cancelTokenSource.CancelAsync();
    }

    public async Task StartBackupAsync(SocketInteractionContext interactionContext,
        ServiceResponseHandler responseHandler, bool isUntilLast)
    {
        _responseHandler = responseHandler;
        var author = _mapper.Map(interactionContext.User);
        var channel = _mapper.Map(interactionContext.Channel);
        var backupRegistry = await RegisterNewBackup(channel, author);
        _context = _contextFactory.RegisterNewBackup(interactionContext, isUntilLast, backupRegistry);

        _profiler.Subscribe(nameof(CompressFiles));
        var saveTimer = _profiler.Subscribe(TimerSaveMessageId);
        var downloadTimer = _profiler.Subscribe(TimerDownloadId);

        _batchManager = _batchManagerFactory.Create(_context, interactionContext.Channel, saveTimer, downloadTimer);

        try
        {
            await _responseHandler.SendStartNotificationAsync(_context);

            await BackupMessages();
            await CompressFiles();
            await UpdateFullStatisticData();
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
            _profiler.TotalElapsed(true, TimerSaveMessageId, TimerDownloadId).Formatted(),
            ByteSizeConversor.ToFormattedString(_context.StatisticTracker.CompressedFilesSize)
        );

        await _responseHandler.SendCompletedAsync(_context, _context.BackupRegistry.Channel);
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

            var batch = await _batchManager.StartBatchingAsync(lastMessageId, _cancelToken);

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

            await _batchManager.SaveBatchAsync(batch, _cancelToken);
            timer.Stop();

            await FinishBatch(timer, batch);
            lastMessageId = batch.RawMessages.Last().Id;
        }
    }

    private async Task FinishBatch(Timer timer, BackupBatch batch)
    {
        _context.MessageCount += batch.ProcessedMessages.Count();
        _logger.BatchFinished(timer, batch.Number);
        await _responseHandler.SendBatchFinishedAsync(_context, batch, timer.Mean);
    }

    //TODO: Implement a way of tracking the progress of the compression
    private async Task CompressFiles() 
    {
        _logger.Log.Information("Compressing files");
        await _responseHandler.SendCompressingFilesAsync(_context);

        var timer = _profiler.Timers[nameof(CompressFiles)].StartTimer();

        await _backupCompressor.CompressAsync(_context, _cancelToken);
        
        timer.Stop();
        _logger.Log.Information("Files compressed in {seconds}", timer.Elapsed.Formatted());
    }

    private async Task UpdateFullStatisticData()
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

    private async Task<BackupRegistry> RegisterNewBackup(Channel channel, User author)
    {
        var backupRegistry = new BackupRegistry
        {
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };

        if (!await _dbContext.Channels.AnyAsync(c => c.Id == channel.Id))
        {
            _dbContext.Channels.Add(channel);
            backupRegistry.Channel = channel;
        }
        else
        {
            backupRegistry.Channel = await _dbContext.Channels.FirstAsync(c => c.Id == channel.Id);
        }


        if (!await _dbContext.Users.AnyAsync(u => u.Id == author.Id))
        {
            _dbContext.Users.Add(author);
            backupRegistry.Author = author;
        }
        else
        {
            backupRegistry.Author = await _dbContext.Users.FirstAsync(u => u.Id == author.Id);
        }

        _dbContext.BackupRegistries.Add(backupRegistry);
        await _dbContext.SaveChangesAsync();

        return backupRegistry;
    }
}