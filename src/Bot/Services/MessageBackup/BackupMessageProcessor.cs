using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;

namespace OPZBot.Services.MessageBackup;

public class BackupMessageProcessor : IBackupMessageProcessor
{
    private readonly MyDbContext _dataContext;
    private readonly IdCacheManager _cache;
    public event Action? FinishBackupProcess;

    public bool IsUntilLastBackup { get; set; }

    public BackupMessageProcessor(MyDbContext dataContext, IdCacheManager cache)
    {
        _dataContext = dataContext;
        _cache = cache;
    }

    public async Task<MessageDataBatchDto> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch)
    {
        var users = new List<IUser>();
        var messages = new List<IMessage>();

        foreach (var message in messageBatch)
        {
            if (await _dataContext.Messages.AnyAsync(m => message.Id == m.Id)) //TODO Machinegun database spammer
            {
                if (IsUntilLastBackup)
                {
                    FinishBackupProcess?.Invoke();
                    break;
                }

                continue;
            }

            if (!await _cache.UserIds.ExistsAsync(message.Author.Id))
                users.Add(message.Author);

            messages.Add(message);
        }

        return new MessageDataBatchDto(users, messages);
    }
}