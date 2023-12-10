using Discord;
using Discord.Interactions;
using OPZBot.Core.Entities;
using OPZBot.DataAccess;

namespace OPZBot.Bot.Services.MessageBackup;

public class BackupService
{
    private readonly MessageFetcher _messageFetcher;
    private readonly BackupMessageProcessor _messageProcessor;
    private readonly AutoMapper _mapper;
    private readonly BackupDataService _dataService;
    private SocketInteractionContext _command;
    private uint _backupId;

    public BackupService(MessageFetcher messageFetcher, AutoMapper mapper, BackupMessageProcessor messageProcessor, BackupDataService dataService)
    {
        _messageFetcher = messageFetcher;
        _mapper = mapper;
        _messageProcessor = messageProcessor;
        _dataService = dataService;
        _messageProcessor.FinishBackupProcess += StopBackup;
    }

    public async Task Start(SocketInteractionContext command, bool untilLastBackup)
    {
        _command = command;
        _messageProcessor.UntilLastBackup = untilLastBackup;

        //Build Channel, Author, BackupRegister
        var channel = _mapper.Map(command.Channel);
        var author = _mapper.Map(command.User);

        var registry = new BackupRegistry()
        {
            Id = 0,
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };
        
        
        _backupId = registry.Id;
        await _dataService.SaveIfNotExistsAsync(channel);
        await _dataService.SaveAsync(registry);
        await _dataService.SaveIfNotExistsAsync(author);
        
        await StartBackupMessages();
    }

    private bool _continueBackup = true;

    private async Task StartBackupMessages()
    {
        ulong lastMessageId = 0;
        while (_continueBackup)
        {
            IEnumerable<IMessage> fetchedMessages;

            if (lastMessageId == 0)
                fetchedMessages = await _messageFetcher.Fetch(_command.Channel);
            else
                fetchedMessages = await _messageFetcher.Fetch(_command.Channel, lastMessageId);

            if (!fetchedMessages.Any()) break;
            lastMessageId = fetchedMessages.Last().Id;

            var processedBackup = await _messageProcessor.ProcessMessages(fetchedMessages, _backupId);
            if (!processedBackup.Messages.Any()) continue;

            await SaveBatch(processedBackup);
        }

        //Finalize backup process
    }
    
    private async Task SaveBatch(ProcessedMessageData processedMessageData)
    {
        await _dataService.SaveIfNotExistsAsync(processedMessageData.Users);
        await _dataService.SaveAsync(processedMessageData.Messages);
    }

    private void StopBackup()
        => _continueBackup = false;
}
