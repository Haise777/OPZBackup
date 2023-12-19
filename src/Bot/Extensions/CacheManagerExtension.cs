using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using Serilog;

namespace OPZBot.Extensions;

public static class CacheManagerExtension
{
    public static async Task SynchronizeCacheAsync(this IdCacheManager cacheManager, MyDbContext context)
    {
        Log.Information("{service} Cache has been synchronized", "Cache:");
        await cacheManager.ChannelIds.UpdateRangeAsync(
            await context.Channels.Select(c => c.Id).ToArrayAsync());
        await cacheManager.UserIds.UpdateRangeAsync(
            await context.Users.Select(u => u.Id).ToArrayAsync());
    }
}