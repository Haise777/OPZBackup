using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;

namespace OPZBot.Services.MessageBackup;

public class BackupMessageProcessor : IBackupMessageProcessor
{
    private readonly Mapper _mapper;
    private readonly MyDbContext _dataContext;
    private readonly IdCacheManager _cache;
    public event Action? FinishBackupProcess;
    
    public bool IsUntilLastBackup { get; set; }
    
    public BackupMessageProcessor(Mapper mapper, MyDbContext dataContext, IdCacheManager cache)
    {
        _mapper = mapper;
        _dataContext = dataContext;
        _cache = cache;
    }

    public async Task<MessageDataBatch> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint backupId)
    {
        var processedMessages = new MessageDataBatch();
        
        foreach (var message in messageBatch)
        {
            if (await _dataContext.Messages.AnyAsync(m => message.Id == m.Id))
            {
                if (IsUntilLastBackup)
                {
                    FinishBackupProcess?.Invoke();
                    break;
                }

                continue;
            }
            
            if (!await _cache.UserIds.ExistsAsync(message.Author.Id)) 
                processedMessages.Users.Add(message.Author);

            processedMessages.Messages.Add(message);
        }

        return processedMessages;
    }
}