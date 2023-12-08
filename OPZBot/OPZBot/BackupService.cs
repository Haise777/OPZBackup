using Discord;
using Discord.Interactions;

namespace OPZBot;

public class BackupService
{
    private readonly BackupRegister _register;
    private readonly MessageFetcher _messageFetcher;
    private SocketInteractionContext _command;
    private bool _untilLast;
    private List<User> _users;
    private AutoMapper _mapper;

    public BackupService(MessageFetcher messageFetcher, AutoMapper mapper)
    {
        _messageFetcher = messageFetcher;
        _mapper = mapper;
    }

    public async Task Start(SocketInteractionContext command, bool untilLast)
    {
        _command = command;
        _untilLast = untilLast;
        _users = new List<User>() { _mapper.Map<User>(command.User) };
        
        //Build Channel, Author, BackupRegister

        await MakeBackup();
    }

    private async Task MakeBackup()
    {
        ulong lastMessage = 0;
        bool continueBackup = true;
        while (continueBackup)
        {
            var fetchedMessages = await _messageFetcher.Fetch(_command.Channel, lastMessage);
            if (!fetchedMessages.Any()) break;
            var messageBatch = await ProcessMessages(fetchedMessages, out continueBackup);
            if (!messageBatch.Any()) continue;

            await SaveBatch(messageBatch);
        }
    }

    private async Task SaveBatch(IEnumerable<Messages> messageBatch)
    {
        
    }

    private Task<IEnumerable<Messages>> ProcessMessages(IEnumerable<IMessage> messageBatch, out bool continueBackup)
    {
        continueBackup = true;
        var filteredMessages = new List<Message>();
        foreach (var message in messageBatch)
        {
            if (_blacklist.Has(message.Author.Id)) continue;
            if (_messageRepository.Exists(message))
            {
                continueBackup = false;
                break;
            }
            if (!_users.Exists(x => x.Id == message.Author.Id)) 
            
        }
    }
}