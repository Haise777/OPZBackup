// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPZBackup.Data;
using OPZBackup.Data.Context;

namespace OPZBackup.Services.Blacklist;

public class BlacklistService(
    MyDbContext dataContext,
    IBlacklistResponseHandler responseHandler,
    ILogger<BlacklistService> logger,
    Mapper mapper) : IBlacklistService
{
    public const string SERVICE_NAME = "Blacklist";

    public async Task ListAllAsync(SocketInteraction interaction)
    {
        var blacklisteds = await dataContext.Users
            .Where(u => u.IsBlackListed == true)
            .Select(u => u.Username)
            .ToArrayAsync();

        if (blacklisteds.Length == 0)
        {
            await responseHandler.SendNoUserInBlacklistAsync(interaction);
            return;
        }

        await responseHandler.SendAllUsersAsync(interaction, blacklisteds);
    }

    public async Task RemoveFromAsync(SocketInteraction interaction, SocketUser socketUser)
    {
        var user = await dataContext.Users
            .SingleOrDefaultAsync(e => e.Id == socketUser.Id);

        if (user is null || !user.IsBlackListed)
        {
            logger.LogInformation("{service}: There was no matched user in the blacklist", SERVICE_NAME);
            await responseHandler.SendUserNotExistsAsync(interaction);
            return;
        }

        try
        {
            user.IsBlackListed = false;
            await dataContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            await responseHandler.SendInteractionErrorAsync(interaction);
            throw;
        }

        logger.LogInformation("{service}: User '{user}' was removed from blacklist", SERVICE_NAME, user.Username);
        await responseHandler.SendUserRemovedAsync(interaction, user.Username);
    }

    public async Task AddToBlacklistAsync(SocketInteraction interaction, SocketUser socketUser)
    {
        var user = await dataContext.Users.SingleOrDefaultAsync(u => u.Id == socketUser.Id);
        if (user is null)
        {
            user = mapper.Map(socketUser);
            dataContext.Users.Add(user);
        }
        else if (user.IsBlackListed)
        {
            logger.LogInformation("{service}: User '{user}' was already added to blacklist", SERVICE_NAME,
                user.Username);
            await responseHandler.SendUserAlreadyAddedAsync(interaction);
            return;
        }

        try
        {
            user.IsBlackListed = true;
            await dataContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            await responseHandler.SendInteractionErrorAsync(interaction);
            throw;
        }

        logger.LogInformation("{service}: User '{user}' was added to blacklist", SERVICE_NAME, user.Username);
        await responseHandler.SendUserAddedAsync(interaction, user.Username);
    }
}