using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;
using OPZBot.Logging;
using OPZBot.Utilities;

namespace OPZBot.Services.MessageBackup;

public class BackupMessageService : BackupService
{
    private readonly ILogger<BackupMessageService> _logger;
    private readonly IMessageFetcher _messageFetcher;
    private readonly IBackupMessageProcessor _messageProcessor;
    private bool _continueBackup = true;

    public int BatchNumber { get; private set; }
    public int SavedMessagesCount { get; private set; }

    public BackupMessageService(IMessageFetcher messageFetcher, Mapper mapper, IBackupMessageProcessor messageProcessor,
        MyDbContext dataContext, IdCacheManager cache, ILogger<BackupMessageService> logger)
        : base(mapper, dataContext, cache)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _logger = logger;
        _messageProcessor.FinishBackupProcess += StopBackup;
    }

    public event AsyncEventHandler<BackupEventArgs>? StartedBackupProcess;
    public event AsyncEventHandler<BackupEventArgs>? FinishedBatch;
    public event AsyncEventHandler<BackupEventArgs>? CompletedBackupProcess;
    public event AsyncEventHandler<BackupEventArgs>? ProcessHasFailed;
    public event AsyncEventHandler<BackupEventArgs>? InvalidBackupAttempt; //TODO To implement

    public async Task StartBackupAsync(SocketInteractionContext context, bool isUntilLastBackup)
    {
        _messageProcessor.IsUntilLastBackup = isUntilLastBackup;
        await base.StartBackupAsync(context);
        try
        {
            await StartedBackupProcess.InvokeAsync(this, new BackupEventArgs(context, BackupRegistry));
            await StartBackupMessages();
        }
        catch (Exception)
        {
            if (BackupRegistry is not null)
            {
                DataContext.BackupRegistries.Remove(BackupRegistry);
                await DataContext.SaveChangesAsync();
            }

            await ProcessHasFailed.InvokeAsync(this, new BackupEventArgs(InteractionContext, BackupRegistry));
            throw;
        }

        if (!await DataContext.Messages.AnyAsync(x => x.BackupId == BackupRegistry.Id))
        {
            DataContext.Remove(BackupRegistry);
            await DataContext.SaveChangesAsync();
            await InvalidBackupAttempt.InvokeAsync(this, new BackupEventArgs(context, BackupRegistry));
        }
    }

    private async Task StartBackupMessages()
    {
        IMessage? lastMessage = null;
        var attemptsRemaining = 3;
        while (_continueBackup)
            try
            {
                var fetchedMessages = await FetchMessages(lastMessage);
                if (!fetchedMessages.Any()) break;
                lastMessage = fetchedMessages.Last();

                var messageDataBatch = await _messageProcessor.ProcessMessagesAsync(fetchedMessages);
                if (!messageDataBatch.Messages.Any())
                {
                    _logger.LogInformation(
                        "{service}: Backup {registryId} > Skipped batch as there is no messages valid to save",
                        nameof(BackupService), BackupRegistry.Id);
                    continue;
                }

                await SaveBatch(messageDataBatch);
                await FinishedBatch.InvokeAsync(this,
                    new BackupEventArgs(InteractionContext, BackupRegistry, messageDataBatch));

                UpdateStatistics(messageDataBatch.Messages);
                attemptsRemaining = 3;
            }
            catch (Exception ex)
            {
                if (attemptsRemaining > 0)
                {
                    await _logger.RichLogErrorAsync(
                        ex, "Batching process failed at batch number {batch}, '{remainingAttempts}' attempts remaining",
                        BatchNumber,
                        attemptsRemaining--);
                    await Task.Delay(5000);
                }
                else
                {
                    throw;
                } //TODO make this more readable
            }

        await CompletedBackupProcess.InvokeAsync(this, new BackupEventArgs(InteractionContext, BackupRegistry));
    }

    private async Task<IMessage[]> FetchMessages(IMessage? startingMessage)
    {
        return startingMessage is not null
            ? (await _messageFetcher.Fetch(InteractionContext.Channel, startingMessage.Id)).ToArray()
            : (await _messageFetcher.Fetch(InteractionContext.Channel)).ToArray();
    }

    private async Task SaveBatch(MessageDataBatchDto messageDataBatchDto)
    {
        DataContext.Users.AddRange(Mapper.Map(messageDataBatchDto.Users));
        DataContext.Messages.AddRange(Mapper.Map(messageDataBatchDto.Messages, BackupRegistry.Id));

        await DataContext.SaveChangesAsync();
    }

    private void UpdateStatistics(IEnumerable<IMessage> fetchedMessages)
    {
        SavedMessagesCount += fetchedMessages.Count();
        BatchNumber++;
    }

    private void StopBackup()
    {
        _continueBackup = false;
    }
}