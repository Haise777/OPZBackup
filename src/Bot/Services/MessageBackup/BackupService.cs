using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class BackupService //TODO Is this class too much complex? Does it respect SRP?
{
    private readonly IMessageFetcher _messageFetcher;
    private readonly IBackupMessageProcessor _messageProcessor;
    private readonly MyDbContext _dataContext;
    private readonly IdCacheManager _cache;
    private readonly Mapper _mapper;
    private SocketInteractionContext? _interactionContext;
    private uint _backupId;

    public event Func<SocketInteractionContext,BackupRegistry,Task>? StartedBackupProcess;
    public event Func<SocketInteractionContext,MessageDataBatchDto, Task>? FinishedBatch;
    public event Func<SocketInteractionContext,Task>? CompletedBackupProcess;
    public event Func<SocketInteractionContext, Exception,Task>? ProcessHasFailed;

    public BackupService(IMessageFetcher messageFetcher, Mapper mapper, IBackupMessageProcessor messageProcessor,
        MyDbContext dataContext, IdCacheManager cache)
    {
        _messageFetcher = messageFetcher;
        _mapper = mapper;
        _messageProcessor = messageProcessor;
        _dataContext = dataContext;
        _cache = cache;
        _messageProcessor.FinishBackupProcess += StopBackup;
    }

    public async Task StartBackupAsync(SocketInteractionContext interactionContext, bool isUntilLastBackup)
    {
        _interactionContext = interactionContext;
        _messageProcessor.IsUntilLastBackup = isUntilLastBackup;

        //Build Channel, Author, BackupRegister
        var channel = _mapper.Map(_interactionContext.Channel);
        var author = _mapper.Map(_interactionContext.User);

        _backupId = await _dataContext.BackupRegistries.AnyAsync()
            ? await _dataContext.BackupRegistries
                .OrderByDescending(b => b.Id)
                .Select(x => x.Id)
                .FirstAsync() + 1
            : 1;
        
        var registry = new BackupRegistry()
        {
            Id = _backupId,
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };
        
        if (!await _cache.ChannelIds.ExistsAsync(channel.Id))
            _dataContext.Channels.Add(channel);
        if (!await _cache.UserIds.ExistsAsync(author.Id))
            _dataContext.Users.Add(author);

        _dataContext.BackupRegistries.Add(registry);

        await StartedBackupProcess?.Invoke(_interactionContext, registry);
        try
        {
            await StartBackupMessages();
        }
        catch (Exception ex)
        {
            _dataContext.BackupRegistries.Remove(registry);
            await _dataContext.SaveChangesAsync();
            await ProcessHasFailed?.Invoke(_interactionContext, ex); //TODO Actually implement a proper error handling
            throw;
        }
    }

    private bool _continueBackup = true;

    private void StopBackup()
        => _continueBackup = false;

    private async Task StartBackupMessages()
    {
        ulong lastMessageId = 0;
        while (_continueBackup)
        {
            IEnumerable<IMessage> fetchedMessages;

            if (lastMessageId != 0)
                fetchedMessages = await _messageFetcher.Fetch(_interactionContext.Channel, lastMessageId);
            else
                fetchedMessages = await _messageFetcher.Fetch(_interactionContext.Channel);
            if (!fetchedMessages.Any()) break;
            lastMessageId = fetchedMessages.Last().Id;

            var messageDataBatch = await _messageProcessor.ProcessMessagesAsync(fetchedMessages);
            if (!messageDataBatch.Messages.Any()) continue;

            await SaveBatch(messageDataBatch);
            await FinishedBatch?.Invoke(_interactionContext, messageDataBatch);
        }

        //Finalize backup process
        if (!await _dataContext.Messages.AnyAsync(x => x.BackupId == _backupId))
        {
            var x = await _dataContext.BackupRegistries.FirstAsync(b => b.Id == _backupId);
            _dataContext.Remove(x);
            await _dataContext.SaveChangesAsync();
            //TODO Make a 'there was no message to backup response'
        }
        
        await CompletedBackupProcess?.Invoke(_interactionContext);
    }

    private async Task SaveBatch(MessageDataBatchDto messageDataBatchDto)
    {
        _dataContext.Users.AddRange(_mapper.Map(messageDataBatchDto.Users));
        _dataContext.Messages.AddRange(_mapper.Map(messageDataBatchDto.Messages, _backupId));

        await _dataContext.SaveChangesAsync();
    }
}