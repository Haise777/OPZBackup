using Discord;

namespace OPZBot;


public class BackupMessageProcessor
{
    private readonly AutoMapper _mapper;
    private readonly MessageRepository _messageRepository;
    public bool UntilLastBackup;
    public event Action FinishBackupProcess;
    
    public BackupMessageProcessor(AutoMapper mapper, MessageRepository messageRepository)
    {
        _mapper = mapper;
        _messageRepository = messageRepository;
    }

    public Task<ProcessedMessage> ProcessMessages(IEnumerable<IMessage> messageBatch, uint backupId)
    {
        var processedBackup = new ProcessedMessage();
        
        foreach (var message in messageBatch)
        {
            if (_blacklist.Has(message.Author.Id)) continue;
            if (_messageRepository.Exists(message))
            {
                if (UntilLastBackup)
                {
                    FinishBackupProcess();
                    break;
                }

                continue;
            }

            if (!processedBackup.Users.Exists(x => x.Id == message.Author.Id)) 
                processedBackup.Users.Add(_mapper.Map(message.Author));

            processedBackup.Messages.Add(_mapper.Map(message));
        }

        return Task.FromResult<ProcessedMessage>(processedBackup);
    }
}