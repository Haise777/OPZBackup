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
    private readonly FileBackupService _fileBackup;
    private readonly Mapper _mapper;

    public MessageProcessor(MyDbContext dataContext, IdCacheManager cache, Mapper mapper, FileBackupService fileBackup)
    {
        _dataContext = dataContext;
        _cache = cache;
        _mapper = mapper;
        _fileBackup = fileBackup;
    }

    public event Action? EndBackupProcess;
    public bool IsUntilLastBackup { get; set; }

    public async Task<MessageDataBatchDto> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint registryId)
    {
        var users = new List<User>();
        var messages = new List<Message>();
        var fileCount = 0;

        foreach (var message in messageBatch)
        {
#warning Database call spammer //TODO Machinegun database spammer
            if (await _dataContext.Messages.AnyAsync(m => message.Id == m.Id))
            {
                if (IsUntilLastBackup)
                {
                    EndBackupProcess?.Invoke();
                    break;
                }

                continue;
            }

            var mappedMessage = _mapper.Map(message, registryId);
            if (message.Attachments.Any())
            {
                await _fileBackup.BackupFilesAsync(message);
                mappedMessage.File = @$"{Program.FileBackupPath}\{message.Id}";
                fileCount += message.Attachments.Count;
            }

            if (!await _cache.UserIds.ExistsAsync(message.Author.Id))
                users.Add(_mapper.Map(message.Author));
            messages.Add(mappedMessage);
        }

        return new MessageDataBatchDto(users, messages, fileCount);
    }
}