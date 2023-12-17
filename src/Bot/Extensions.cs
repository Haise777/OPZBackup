using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.Utilities;

namespace OPZBot;

public static class Extensions
{
    public static IServiceCollection AddConfiguredCacheManager(this IServiceCollection services)
    {
        return services.AddSingleton(provider
            => new IdCacheManager(
                new DataCache<ulong>().AddRangeAsync(provider
                    .GetRequiredService<MyDbContext>().Users
                    .Select(u => u.Id)
                    .ToList()).Result,
                new DataCache<ulong>().AddRangeAsync(provider
                    .GetRequiredService<MyDbContext>().Channels
                    .Select(c => c.Id)
                    .ToList()).Result,
                new DataCache<uint>().AddRangeAsync(provider
                    .GetRequiredService<MyDbContext>().BackupRegistries
                    .Select(b => b.Id)
                    .ToList()).Result
            ));
    }
    
    public static Task InvokeAsync<TArgs>(this AsyncEventHandler<TArgs>? eventAsync, object? sender, TArgs e)
    {
        return eventAsync is null 
            ? Task.CompletedTask 
            : Task.WhenAll(eventAsync.GetInvocationList()
                .Cast<AsyncEventHandler<TArgs>>()
                .Select(f => f(sender, e)));
    }

    public static async Task SynchronizeCacheAsync(this IdCacheManager cacheManager, MyDbContext context)
    {
        await cacheManager.ChannelIds.UpdateRangeAsync(
            await context.Channels.Select(c => c.Id).ToArrayAsync());
        await cacheManager.UserIds.UpdateRangeAsync(
            await context.Users.Select(u => u.Id).ToArrayAsync());
    }
}