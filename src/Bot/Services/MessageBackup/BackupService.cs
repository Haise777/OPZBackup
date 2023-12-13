using Discord;
using Discord.Interactions;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class BackupService
{
    private readonly IMessageFetcher _messageFetcher;
    private readonly IBackupMessageProcessor _messageProcessor;
    private readonly MyDbContext _dataContext;
    private readonly IdCacheManager _cache;
    private readonly Mapper _mapper;
    private SocketInteractionContext? _interactionContext;
    private uint _backupId;

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

        var c = _interactionContext.Channel;
        
        var registry = new BackupRegistry()
        {
            Id = 0,
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };
        
        _backupId = registry.Id;
        
        if (!await _cache.ChannelIds.ExistsAsync(channel.Id))
            _dataContext.Channels.Add(channel);
        if (!await _cache.UserIds.ExistsAsync(author.Id))
            _dataContext.Users.Add(author);

        _dataContext.BackupRegistries.Add(registry);

        await StartBackupMessages();
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
            var processedMessageData = await _messageProcessor.ProcessMessagesAsync(fetchedMessages, _backupId);
            if (!processedMessageData.Messages.Any()) continue;

            await SaveBatch(processedMessageData);
        }

        //Finalize backup process
    }

    private async Task SaveBatch(ProcessedMessageData processedMessageData)
    {
        _dataContext.Users.AddRange(processedMessageData.Users);
        _dataContext.Messages.AddRange(processedMessageData.Messages);

        await _dataContext.SaveChangesAsync();
    }
}