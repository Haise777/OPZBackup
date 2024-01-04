// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;
using OPZBot.Extensions;

namespace OPZBot.Services.MessageBackup;

public abstract class BackupService(
    Mapper mapper,
    MyDbContext dataContext,
    IdCacheManager cache,
    FileCleaner fileCleaner)
    : IBackupService
{
    protected readonly IdCacheManager Cache = cache;
    protected readonly MyDbContext DataContext = dataContext;
    protected readonly Mapper Mapper = mapper;
    protected BackupRegistry? BackupRegistry;
    protected SocketInteractionContext? InteractionContext;

    public async Task<TimeSpan> TimeFromLastBackupAsync(SocketInteractionContext interactionContext)
    {
        var lastBackupDate = await DataContext.BackupRegistries
            .Where(b => b.ChannelId == interactionContext.Channel.Id)
            .OrderByDescending(b => b.Date)
            .Select(b => b.Date)
            .FirstOrDefaultAsync();

        return TimeSpan.FromDays(1) - (DateTime.Now - lastBackupDate);
    }

    public virtual async Task DeleteUserAsync(SocketInteractionContext interactionContext)
    {
        var user = await DataContext.Users
            .SingleOrDefaultAsync(u => u.Id == interactionContext.User.Id);
        if (user is null)
        {
            user = Mapper.Map(interactionContext.User);
            DataContext.Users.Add(user);
        }

        user.IsBlackListed = true;

        var messages = await DataContext.Messages.Where(m => m.AuthorId == user.Id).ToArrayAsync();
        DataContext.Messages.RemoveRange(messages);
        await DataContext.SaveChangesAsync();
        await fileCleaner.DeleteMessageFilesAsync(messages);
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

        await AddIfNotExists(channel, author);
        DataContext.BackupRegistries.Add(BackupRegistry);
        await DataContext.SaveChangesAsync();
    }

    private async Task AddIfNotExists(Channel channel, User author)
    {
        if (!await Cache.ChannelIds.ExistsAsync(channel.Id))
            DataContext.Channels.Add(channel);
        if (!await Cache.Users.ExistsAsync(author.Id))
            DataContext.Users.Add(author);
    }

    private async Task<uint> GetIncrementedRegistryId()
    {
        return await DataContext.BackupRegistries.AnyAsync()
            ? await DataContext.BackupRegistries
                .OrderByDescending(b => b.Id)
                .Select(x => x.Id)
                .FirstAsync() + 1
            : 1;
    }
}