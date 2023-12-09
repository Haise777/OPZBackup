using Discord;
using Discord.Interactions;

namespace OPZBot;

public class BackupService
{
    private readonly MessageFetcher _messageFetcher;
    private readonly BackupMessageProcessor _messageProcessor;
    private readonly AutoMapper _mapper;
    private SocketInteractionContext _command;
    private uint _backupId;

    public BackupService(MessageFetcher messageFetcher, AutoMapper mapper, BackupMessageProcessor messageProcessor)
    {
        _messageFetcher = messageFetcher;
        _mapper = mapper;
        _messageProcessor = messageProcessor;
        _messageProcessor.FinishBackupProcess += StopBackup;
    }

    public async Task Start(SocketInteractionContext command, bool untilLastBackup)
    {
        _command = command;
        _messageProcessor.UntilLastBackup = untilLastBackup;

        //Build Channel, Author, BackupRegister
        var channel = _mapper.Map(command.Channel);
        var author = _mapper.Map(command.User);

        register = new BackupRegister()
        {
            Id = 0,
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };

        _backupId = register.Id;
        await _channelRepository.SaveIfNotExists(channel);
        await _backupRegisterRepository.Save(register);

        _users = new List<User>() { author };

        await BackupMessages();
    }

    private bool _continueBackup = true;

    private async Task BackupMessages()
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


    private async Task SaveBatch(ProcessedBackup processedBackup)
    {
        await _usersRepository.SaveIfNotExists(processedBackup.Users);
        await _messageRepository.Save(processedBackup.Messages);
    }

    private void StopBackup()
        => _continueBackup = false;
}
