// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

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
        await cacheManager.Users.UpdateRangeAsync(
            await context.Users.Select(u => u.Id).ToArrayAsync());
    }
}