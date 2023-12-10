using OPZBot.Core.Contracts;
using Discord;

namespace OPZBot.Bot.Services.MessageBackup;

public class BackupMessageProcessor
{
    private readonly AutoMapper _mapper;
    private readonly IMessageRepository _messageRepository;
    public bool UntilLastBackup;
    public event Action FinishBackupProcess;
    
    public BackupMessageProcessor(AutoMapper mapper, IMessageRepository messageRepository)
    {
        _mapper = mapper;
        _messageRepository = messageRepository;
    }

    public async Task<ProcessedMessageData> ProcessMessages(IEnumerable<IMessage> messageBatch, uint backupId)
    {
        var processedBackup = new ProcessedMessageData();
        
        foreach (var message in messageBatch)
        {
            if (!await _messageRepository.ExistsAsync(m => m.Id == message.Id))
            {
                if (UntilLastBackup)
                {
                    FinishBackupProcess();
                    break;
                }

                continue;
            }

            if (!processedBackup.Users.Exists(u => u.Id == message.Author.Id)) 
                processedBackup.Users.Add(_mapper.Map(message.Author));

            processedBackup.Messages.Add(_mapper.Map(message));
        }

        return processedBackup;
    }
}