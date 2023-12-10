using OPZBot.Core.Contracts;
using OPZBot.Core.Entities;

namespace OPZBot.DataAccess;

public class BackupDataService
{
    private readonly IChannelRepository _channelRepository;
    private readonly IBackupRegistryRepository _registryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;

    public BackupDataService(IChannelRepository channelRepository, IBackupRegistryRepository registryRepository,
        IUserRepository userRepository, IMessageRepository messageRepository)
    {
        _channelRepository = channelRepository;
        _registryRepository = registryRepository;
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }

    public async Task SaveIfNotExistsAsync(Channel channel)
    {
        await _channelRepository.SaveIfNotExistsAsync(channel);
    }
    
    public async Task SaveIfNotExistsAsync(User author)
    {
        await _userRepository.SaveIfNotExistsAsync(author);
    }
    
    public async Task SaveIfNotExistsAsync(IEnumerable<User> authors)
    {
        await _userRepository.SaveIfNotExistsAsync(authors);
    }

    public async Task SaveAsync(BackupRegistry registry)
    {
        await _registryRepository.SaveAsync(registry);
    }
    
    public async Task SaveAsync(Message message)
    {
        await _messageRepository.SaveAsync(message);
    }
    public async Task SaveAsync(IEnumerable<Message> messages)
    {
        await _messageRepository.SaveAsync(messages);
    }
}