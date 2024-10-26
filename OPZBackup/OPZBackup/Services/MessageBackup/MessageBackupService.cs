// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPZBackup.DataAccess;
using OPZBackup.DataAccess.Caching;
using OPZBackup.DataAccess.Context;
using OPZBackup.Utilities;
using OPZBackup.Extensions;
using OPZBackup.Logging;

namespace OPZBackup.Services.MessageBackup;

public class MessageBackupService : BackupService, IMessageBackupService
{
    private readonly FileCleaner _fileCleaner;
    private readonly ILogger<MessageBackupService> _logger;
    private readonly IMessageFetcher _messageFetcher;
    private readonly IBackupMessageProcessor _messageProcessor;
    private bool _continueBackup = true;

    public MessageBackupService(IMessageFetcher messageFetcher, Mapper mapper, IBackupMessageProcessor messageProcessor,
        MyDbContext dataContext, IdCacheManager cache, ILogger<MessageBackupService> logger, FileCleaner fileCleaner)
        : base(mapper, dataContext, cache, fileCleaner)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _logger = logger;
        _fileCleaner = fileCleaner;
        _messageProcessor.EndBackupProcess += StopBackup;
    }

    public int BatchNumber { get; private set; }
    public int SavedMessagesCount { get; private set; }
    public int SavedFilesCount { get; private set; }

    public CancellationTokenSource CancelSource { get; } = new();

    public event AsyncEventHandler<BackupEventArgs>? StartedBackupProcess;
    public event AsyncEventHandler<BackupEventArgs>? FinishedBatch;
    public event AsyncEventHandler<BackupEventArgs>? CompletedBackupProcess;
    public event AsyncEventHandler<BackupEventArgs>? ProcessFailed;
    public event AsyncEventHandler<BackupEventArgs>? EmptyBackupAttempt;
    public event AsyncEventHandler<BackupEventArgs>? ProcessCanceled;

    public async Task StartBackupAsync(SocketInteractionContext context, bool isUntilLastBackup)
    {
        _messageProcessor.IsUntilLastBackup = isUntilLastBackup;
        await base.StartBackupAsync(context);

        try
        {
            await StartedBackupProcess.InvokeAsync(this, new BackupEventArgs(context, BackupRegistry));
            await StartBackupMessages();
        }
        catch (OperationCanceledException)
        {
            await ProcessCanceled.InvokeAsync(this, new BackupEventArgs(InteractionContext, BackupRegistry));
            await CleanupBackupLeftovers();
        }
        catch (Exception)
        {
            await ProcessFailed.InvokeAsync(this, new BackupEventArgs(InteractionContext, BackupRegistry));
            await CleanupBackupLeftovers();
            throw;
        }
    }

    private async Task StartBackupMessages()
    {
        IMessage? lastMessage = null;
        var attempts = 0;
        while (_continueBackup)
            try
            {
                CancelSource.Token.ThrowIfCancellationRequested();
                var fetchedMessages = await FetchMessages(lastMessage);
                if (fetchedMessages.Length == 0) break;
                lastMessage = fetchedMessages.Last();

                var messageDataBatch = await _messageProcessor.ProcessMessagesAsync(
                    fetchedMessages, BackupRegistry!.Id, CancelSource.Token);
                if (IsEmptyBatch(messageDataBatch)) continue;

                await SaveBatch(messageDataBatch);
                UpdateStatistics(messageDataBatch);
                await FinishedBatch.InvokeAsync(this,
                    new BackupEventArgs(InteractionContext, BackupRegistry, messageDataBatch));

                attempts = 0;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (++attempts > 3) throw;
                await BatchFailed(ex, attempts - 4);
            }

        if (await CheckIfBackupIsEmpty()) return;
        await CompletedBackupProcess.InvokeAsync(this, new BackupEventArgs(InteractionContext, BackupRegistry));
    }

    private async Task<IMessage[]> FetchMessages(IMessage? startingMessage)
    {
        return startingMessage is not null
            ? (await _messageFetcher.FetchAsync(InteractionContext!.Channel, startingMessage.Id)).ToArray()
            : (await _messageFetcher.FetchAsync(InteractionContext!.Channel)).ExcludeFirst().ToArray();
    }

    private bool IsEmptyBatch(MessageBatchData batch)
    {
        if (batch.Messages.Any()) return false;

        _logger.LogInformation(
            "{service}: Backup {registryId} > Skipped batch as there is no messages valid to save",
            nameof(BackupService), BackupRegistry!.Id);
        return true;
    }

    private async Task SaveBatch(MessageBatchData messageBatchData)
    {
        DataContext.Users.AddRange(messageBatchData.Users);
        DataContext.Messages.AddRange(messageBatchData.Messages);

        await DataContext.SaveChangesAsync();
    }

    private void UpdateStatistics(MessageBatchData batchDataBatch)
    {
        SavedMessagesCount += batchDataBatch.Messages.Count();
        SavedFilesCount += batchDataBatch.FileCount;
        BatchNumber++;
    }

    private async Task BatchFailed(Exception ex, int attemptsRemaining)
    {
        await _logger.RichLogErrorAsync(
            ex, "Batching process failed at batch number {batch}, '{remainingAttempts}' attempts remaining",
            BatchNumber,
            attemptsRemaining);
        await Task.Delay(5000);
    }

    private async Task<bool> CheckIfBackupIsEmpty()
    {
        if (await DataContext.Messages.AnyAsync(x => x.BackupId == BackupRegistry!.Id))
            return false;

        DataContext.Remove(BackupRegistry!);
        await DataContext.SaveChangesAsync();
        await EmptyBackupAttempt.InvokeAsync(this, new BackupEventArgs(InteractionContext, BackupRegistry));
        return true;
    }

    private async Task CleanupBackupLeftovers()
    {
        if (BackupRegistry is not null)
        {
            DataContext.BackupRegistries.Remove(BackupRegistry);
            await _fileCleaner.DeleteMessageFilesAsync(await DataContext.Messages
                .Where(m => m.BackupId == BackupRegistry.Id)
                .ToArrayAsync());
            await DataContext.SaveChangesAsync();
        }
    }

    private void StopBackup() => _continueBackup = false;
}