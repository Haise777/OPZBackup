using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public abstract class BackupService
{
    protected readonly IdCacheManager Cache;
    protected readonly MyDbContext DataContext;
    protected readonly Mapper Mapper;
    protected BackupRegistry BackupRegistry;
    protected SocketInteractionContext InteractionContext;

    protected BackupService(Mapper mapper, MyDbContext dataContext, IdCacheManager cache)
    {
        Mapper = mapper;
        DataContext = dataContext;
        Cache = cache;
    }

    protected virtual async Task StartBackupAsync(SocketInteractionContext interactionContext)
    {
        await Cache.SynchronizeCacheAsync(DataContext);
        InteractionContext = interactionContext;

        var channel = Mapper.Map(InteractionContext.Channel);
        var author = Mapper.Map(InteractionContext.User);

        BackupRegistry = new BackupRegistry
        {
            Id = await GetIncrementedRegistryId(),
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };

        if (!await Cache.ChannelIds.ExistsAsync(channel.Id))
            DataContext.Channels.Add(channel);
        if (!await Cache.UserIds.ExistsAsync(author.Id))
            DataContext.Users.Add(author);

        DataContext.BackupRegistries.Add(BackupRegistry);
        await DataContext.SaveChangesAsync();
    }

    public async Task<TimeSpan> TimeFromLastBackupAsync(SocketInteractionContext context)
    {
        var lastBackupDate = await DataContext.BackupRegistries
            .Where(b => b.ChannelId == context.Channel.Id)
            .OrderByDescending(b => b.Date)
            .Select(b => b.Date)
            .FirstOrDefaultAsync();

        return DateTime.Now - lastBackupDate;
    }

    public virtual async Task DeleteUserAsync(ulong userId)
    {
        var user = await DataContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        DataContext.Users.Remove(user);
        await DataContext.SaveChangesAsync();
    }

    private async Task<uint> GetIncrementedRegistryId()
    {
        var registryId = await DataContext.BackupRegistries.AnyAsync()
            ? await DataContext.BackupRegistries
                .OrderByDescending(b => b.Id)
                .Select(x => x.Id)
                .FirstAsync() + 1
            : 1;
        return registryId;
    }
}