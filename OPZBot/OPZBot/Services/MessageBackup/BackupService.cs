using Discord;
using Discord.Interactions;

namespace OPZBot;

public class BackupService
{
    private readonly MessageFetcher _messageFetcher;
    private readonly AutoMapper _mapper;
    private BackupRegister _register;
    private bool _untilLastBackup;
    private List<User> _users;
    private SocketInteractionContext _command;

    public BackupService(MessageFetcher messageFetcher, AutoMapper mapper)
    {
        _messageFetcher = messageFetcher;
        _mapper = mapper;
    }

    public async Task Start(SocketInteractionContext command, bool untilLastBackup)
    {
        _command = command;
        _untilLastBackup = untilLastBackup;

        //Build Channel, Author, BackupRegister
        var channel = _mapper.Map(command.Channel);
        var author = _mapper.Map(command.User);

        _register = new BackupRegister()
        {
            Id = 0,
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };

        await _channelRepository.SaveIfNotExists(channel);
        await _backupRegisterRepository.Save(_register);

        _users = new List<User>() { author };

        await MakeBackup();
    }

    private bool _continueBackup = true;

    private async Task MakeBackup()
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

            var messageBatch = await ProcessMessages(fetchedMessages);
            if (!messageBatch.Any()) continue;

            await SaveBatch(messageBatch);
        }

        //Finalize backup process
    }

    private Task<IEnumerable<Messages>> ProcessMessages(IEnumerable<IMessage> messageBatch)
    {
        var filteredMessages = new List<Message>();
        foreach (var message in messageBatch)
        {
            if (_blacklist.Has(message.Author.Id)) continue;
            if (_messageRepository.Exists(message))
            {
                if (_untilLastBackup)
                {
                    _continueBackup = false;
                    break;
                }

                continue;
            }

            if (!_users.Exists(x => x.Id == message.Author.Id)) _users.Add(_mapper.Map(message.Author));

            filteredMessages.Add(_mapper.Map(message));
        }

        return Task.FromResult<IEnumerable<Messages>>(filteredMessages);
    }

    private async Task SaveBatch(IEnumerable<Messages> messageBatch)
    {
        await _usersRepository.SaveIfNotExists(_users);
        await _messageRepository.Save(messageBatch);
    }
}
