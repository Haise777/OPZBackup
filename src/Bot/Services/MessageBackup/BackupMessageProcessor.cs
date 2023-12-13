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
    public bool IsUntilLastBackup { get; set; }
    public event Action? FinishBackupProcess;
    
    public BackupMessageProcessor(Mapper mapper, MyDbContext dataContext, IdCacheManager cache)
    {
        _mapper = mapper;
        _dataContext = dataContext;
        _cache = cache;
    }

    public async Task<ProcessedMessageData> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint backupId)
    {
        var processedMessages = new ProcessedMessageData();
        
        foreach (var message in messageBatch)
        {
            if (!await _dataContext.Messages.AnyAsync(m => m.Id == message.Id))
            {
                if (IsUntilLastBackup)
                {
                    FinishBackupProcess();
                    break;
                }

                continue;
            }
            
            if (!await _cache.UserIds.ExistsAsync(message.Author.Id)) 
                processedMessages.Users.Add(_mapper.Map(message.Author));

            processedMessages.Messages.Add(_mapper.Map(message));
        }

        return processedMessages;
    }
}