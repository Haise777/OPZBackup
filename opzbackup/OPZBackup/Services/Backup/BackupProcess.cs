﻿using Discord;
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
    private readonly CancellationToken _cancelToken;
    private readonly CancellationTokenSource _cancelTokenSource;
    private readonly BackupContextFactory _contextFactory;
    private readonly MyDbContext _dbContext;
    private readonly BackupLogger _logger;
    private readonly Timer _performanceTimer;
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
        BackupLogger logger,
        BatchManagerFactory batchManagerFactory,
        Mapper mapper,
        BackupCompressor backupCompressor, Timer performanceTimer)
    {
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _logger = logger;
        _cancelTokenSource = new CancellationTokenSource();
        _cancelToken = _cancelTokenSource.Token;
        _batchManagerFactory = batchManagerFactory;
        _mapper = mapper;
        _backupCompressor = backupCompressor;
        _performanceTimer = performanceTimer;
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
        await InitialSetup(interactionContext, responseHandler, isUntilLast);

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
            _performanceTimer.Total.Formatted(),
            ByteSizeConversor.ToFormattedString(_context.StatisticTracker.CompressedFilesSize)
        );

        await _responseHandler.SendCompletedAsync(_context, _context.BackupRegistry.Channel);
    }

    private async Task InitialSetup(SocketInteractionContext interactionContext,
        ServiceResponseHandler responseHandler, bool isUntilLast)
    {
        var author = _mapper.Map(interactionContext.User);
        var channel = _mapper.Map(interactionContext.Channel);
        var backupRegistry = await RegisterNewBackup(channel, author);
        
        _responseHandler = responseHandler;
        _context = _contextFactory.Create(interactionContext, isUntilLast, backupRegistry);
        _batchManager = _batchManagerFactory.Create(_context, interactionContext.Channel);
    }

    private async Task BackupMessages()
    {
        _logger.Log.Information("Starting backup");
        ulong lastMessageId = 0;

        while (true)
        {
            _cancelToken.ThrowIfCancellationRequested();
            _performanceTimer.StartTimer();

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
            _performanceTimer.Stop();

            await FinishBatch(batch);
            lastMessageId = batch.RawMessages.Last().Id;
        }
    }

    private async Task FinishBatch(BackupBatch batch)
    {
        _context.MessageCount += batch.ProcessedMessages.Count();
        _logger.BatchFinished(_performanceTimer, batch.Number);
        await _responseHandler.SendBatchFinishedAsync(_context, batch, _performanceTimer.Mean);
    }

    //TODO: Implement a way of tracking the progress of the compression
    private async Task CompressFiles()
    {
        await _responseHandler.SendCompressingFilesAsync(_context);
        await _backupCompressor.CompressAsync(_context, _cancelToken);
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