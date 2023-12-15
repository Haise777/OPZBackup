using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class MessageBackupService : BackupService
{
    public event Func<SocketInteractionContext, BackupRegistry, Task>? StartedBackupProcess;
    public event Func<SocketInteractionContext, MessageDataBatchDto, Task>? FinishedBatch;
    public event Func<SocketInteractionContext, Task>? CompletedBackupProcess;
    public event Func<SocketInteractionContext, Exception, Task>? ProcessHasFailed;

    private readonly IMessageFetcher _messageFetcher;
    private readonly IBackupMessageProcessor _messageProcessor;
    private bool _continueBackup = true;

    public MessageBackupService(IMessageFetcher messageFetcher, Mapper mapper, IBackupMessageProcessor messageProcessor,
        MyDbContext dataContext, IdCacheManager cache)
        : base(mapper, dataContext, cache)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _messageProcessor.FinishBackupProcess += StopBackup;
    }

    public async Task StartBackupAsync(SocketInteractionContext context, bool isUntilLastBackup)
    {
        _messageProcessor.IsUntilLastBackup = isUntilLastBackup;
        await base.StartBackupAsync(context);
        await StartedBackupProcess?.Invoke(context, BackupRegistry);
        
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
            await ProcessHasFailed?.Invoke(InteractionContext, ex); //TODO Actually implement a proper error handling
            throw;
        }
    }

    private void StopBackup()
        => _continueBackup = false;

    private async Task StartBackupMessages()
    {
        IMessage? lastMessage = null;
        while (_continueBackup)
        {
            IMessage[] fetchedMessages;

            if (lastMessage is not null)
                fetchedMessages = (await _messageFetcher.Fetch(InteractionContext.Channel, lastMessage.Id)).ToArray();
            else
                fetchedMessages = (await _messageFetcher.Fetch(InteractionContext.Channel)).ToArray();
            if (!fetchedMessages.Any()) break;
            
            var messageDataBatch = await _messageProcessor.ProcessMessagesAsync(fetchedMessages);
            
            if (!messageDataBatch.Messages.Any()) continue;

            await SaveBatch(messageDataBatch);
            await FinishedBatch?.Invoke(InteractionContext, messageDataBatch);
            lastMessage = fetchedMessages.Last();
        }

        //Finalize backup process
        await CompletedBackupProcess?.Invoke(InteractionContext);
    }
    
    private async Task SaveBatch(MessageDataBatchDto messageDataBatchDto)
    {
        DataContext.Users.AddRange(Mapper.Map(messageDataBatchDto.Users));
        DataContext.Messages.AddRange(Mapper.Map(messageDataBatchDto.Messages, BackupRegistry.Id));

        await DataContext.SaveChangesAsync();
    }
}