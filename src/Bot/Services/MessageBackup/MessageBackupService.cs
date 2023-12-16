using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;
using OPZBot.Logging;

namespace OPZBot.Services.MessageBackup;

public class MessageBackupService : BackupService
{
    private readonly IMessageFetcher _messageFetcher;
    private readonly IBackupMessageProcessor _messageProcessor;
    private readonly ILogger<MessageBackupService> _logger;
    private bool _continueBackup = true;

    public MessageBackupService(IMessageFetcher messageFetcher, Mapper mapper, IBackupMessageProcessor messageProcessor,
        MyDbContext dataContext, IdCacheManager cache, ILogger<MessageBackupService> logger)
        : base(mapper, dataContext, cache)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _logger = logger;
        _messageProcessor.FinishBackupProcess += StopBackup;
    }

    public event Func<SocketInteractionContext, BackupRegistry, Task>? StartedBackupProcess;
    public event Func<SocketInteractionContext, MessageDataBatchDto, BackupRegistry, Task>? FinishedBatch;
    public event Func<SocketInteractionContext, BackupRegistry, Task>? CompletedBackupProcess;
    public event Func<SocketInteractionContext, Exception, BackupRegistry, Task>? ProcessHasFailed;

    public async Task StartBackupAsync(SocketInteractionContext context, bool isUntilLastBackup)
    {
        _messageProcessor.IsUntilLastBackup = isUntilLastBackup;
        await base.StartBackupAsync(context);
        if (StartedBackupProcess is not null) await StartedBackupProcess(context, BackupRegistry);

        try
        {
            await StartBackupMessages();

            if (!await DataContext.Messages.AnyAsync(x => x.BackupId == BackupRegistry.Id))
            {
                DataContext.Remove(BackupRegistry);
                await DataContext.SaveChangesAsync();
                //TODO Make a 'there was no message to backup response'
            }
        }
        catch (Exception ex)
        {
            DataContext.BackupRegistries.Remove(BackupRegistry);
            await DataContext.SaveChangesAsync();
            if (ProcessHasFailed is not null) await ProcessHasFailed(InteractionContext, ex, BackupRegistry);
            throw;
        }
    }

    private void StopBackup() => _continueBackup = false;

    private async Task StartBackupMessages()
    {
        IMessage? lastMessage = null;
        var attempts = 3;
        while (_continueBackup)
        {
            try
            {
                var fetchedMessages = lastMessage is not null
                    ? (await _messageFetcher.Fetch(InteractionContext.Channel, lastMessage.Id)).ToArray()
                    : (await _messageFetcher.Fetch(InteractionContext.Channel)).ToArray();

                if (!fetchedMessages.Any()) break;

                var messageDataBatch = await _messageProcessor.ProcessMessagesAsync(fetchedMessages);

                if (!messageDataBatch.Messages.Any()) continue;

                lastMessage = fetchedMessages.Last();
                if (FinishedBatch is not null)
                    await FinishedBatch(InteractionContext, messageDataBatch, BackupRegistry);
                await SaveBatch(messageDataBatch);
                attempts = 3;
            }
            catch (Exception ex)
            {
                if (attempts != -1)
                {
                    await _logger.RichLogErrorAsync(ex,
                        $"Batching process failure, '{--attempts}' attempts remaining");
                    await Task.Delay(5000);
                }
                else throw;
            }
        }

        //Finalize backup process
        if (CompletedBackupProcess is not null) await CompletedBackupProcess(InteractionContext, BackupRegistry);
    }

    private async Task SaveBatch(MessageDataBatchDto messageDataBatchDto)
    {
        DataContext.Users.AddRange(Mapper.Map(messageDataBatchDto.Users));
        DataContext.Messages.AddRange(Mapper.Map(messageDataBatchDto.Messages, BackupRegistry.Id));

        await DataContext.SaveChangesAsync();
    }
}