using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public abstract class BackupService
{
    protected readonly MyDbContext DataContext;
    protected readonly IdCacheManager Cache;
    protected readonly Mapper Mapper;
    protected SocketInteractionContext InteractionContext;
    protected BackupRegistry BackupRegistry;
    
    protected BackupService(Mapper mapper, MyDbContext dataContext, IdCacheManager cache)
    {
        Mapper = mapper;
        DataContext = dataContext;
        Cache = cache;
    }

    protected virtual async Task StartBackupAsync(SocketInteractionContext interactionContext)
    {
        InteractionContext = interactionContext;
        
        var channel = Mapper.Map(InteractionContext.Channel);
        var author = Mapper.Map(InteractionContext.User);
        
        BackupRegistry = new BackupRegistry()
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