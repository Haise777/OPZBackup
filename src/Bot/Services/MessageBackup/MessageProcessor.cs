using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;
using OPZBot.Services.MessageBackup.FileBackup;

namespace OPZBot.Services.MessageBackup;

public class MessageProcessor : IBackupMessageProcessor
{
    private readonly IdCacheManager _cache;
    private readonly MyDbContext _dataContext;
    private readonly Mapper _mapper;
    private FileBackupService _fileBackup;

    public MessageProcessor(MyDbContext dataContext, IdCacheManager cache, Mapper mapper, FileBackupService fileBackup)
    {
        _dataContext = dataContext;
        _cache = cache;
        _mapper = mapper;
        _fileBackup = fileBackup;
    }

    public event Action? FinishBackupProcess;

    public bool IsUntilLastBackup { get; set; }

    public async Task<MessageDataBatchDto> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint registryId)
    {
        var users = new List<User>();
        var messages = new List<Message>();

        foreach (var message in messageBatch)
        {
#warning Database call spammer //TODO Machinegun database spammer
            if (await _dataContext.Messages.AnyAsync(m => message.Id == m.Id))
            {
                if (IsUntilLastBackup)
                {
                    FinishBackupProcess?.Invoke();
                    break;
                }

                continue;
            }

            var mappedMessage = _mapper.Map(message, registryId);
            
            if (message.Attachments.Any())
            {
                await _fileBackup.BackupMessageFilesAsync(message);
                mappedMessage.File = @$"{Program.FileBackupPath}\{message.Id}";
            }

            if (!await _cache.UserIds.ExistsAsync(message.Author.Id))
                users.Add(_mapper.Map(message.Author));
            messages.Add(mappedMessage);
        }

        return new MessageDataBatchDto(users, messages);
    }
}