// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.Extensions;
using OPZBot.Logging;
using OPZBot.Utilities;

namespace OPZBot.Services.MessageBackup;

public class MessageBackupService : BackupService, IMessageBackupService
{
    private readonly ILogger<MessageBackupService> _logger;
    private readonly IMessageFetcher _messageFetcher;
    private readonly IBackupMessageProcessor _messageProcessor;
    private bool _continueBackup = true;

    public MessageBackupService(IMessageFetcher messageFetcher, Mapper mapper, IBackupMessageProcessor messageProcessor,
        MyDbContext dataContext, IdCacheManager cache, ILogger<MessageBackupService> logger)
        : base(mapper, dataContext, cache)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _logger = logger;
        _messageProcessor.EndBackupProcess += StopBackup;
    }

    public CancellationTokenSource CancelSource { get; } = new();
    public int BatchNumber { get; private set; }
    public int SavedMessagesCount { get; private set; }
    public int SavedFilesCount { get; private set; }

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
        var attemptsRemaining = 3;
        while (_continueBackup)
        {
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

                attemptsRemaining = 3;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attemptsRemaining > 0)
                    await BatchFailed(ex, attemptsRemaining--);
                else throw;
            }
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

    private bool IsEmptyBatch(MessageDataBatchDto batch)
    {
        if (batch.Messages.Any()) return false;

        _logger.LogInformation(
            "{service}: Backup {registryId} > Skipped batch as there is no messages valid to save",
            nameof(BackupService), BackupRegistry!.Id);
        return true;
    }

    private async Task SaveBatch(MessageDataBatchDto messageDataBatchDto)
    {
        DataContext.Users.AddRange(messageDataBatchDto.Users);
        DataContext.Messages.AddRange(messageDataBatchDto.Messages);

        await DataContext.SaveChangesAsync();
    }

    private void UpdateStatistics(MessageDataBatchDto dataBatch)
    {
        SavedMessagesCount += dataBatch.Messages.Count();
        SavedFilesCount += dataBatch.FileCount;
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
            await FileCleaner.DeleteMessageFilesAsync(await DataContext.Messages
                .Where(m => m.BackupId == BackupRegistry.Id)
                .ToArrayAsync());
            await DataContext.SaveChangesAsync();
        }
    }

    private void StopBackup() => _continueBackup = false;
}