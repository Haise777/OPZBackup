using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;

namespace OPZBot.Services.MessageBackup;

public class MessageProcessor : IBackupMessageProcessor
{
    private readonly IdCacheManager _cache;
    private readonly MyDbContext _dataContext;

    public MessageProcessor(MyDbContext dataContext, IdCacheManager cache)
    {
        _dataContext = dataContext;
        _cache = cache;
    }

    public event Action? FinishBackupProcess;

    public bool IsUntilLastBackup { get; set; }

    public async Task<MessageDataBatchDto> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch)
    {
        var users = new List<IUser>();
        var messages = new List<IMessage>();

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

            if (!await _cache.UserIds.ExistsAsync(message.Author.Id))
                users.Add(message.Author);

            messages.Add(message);
        }

        return new MessageDataBatchDto(users, messages);
    }
}