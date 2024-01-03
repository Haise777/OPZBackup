// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OPZBot.Logging;
using OPZBot.Services.Blacklist;

namespace OPZBot.Modules;

[Group("blacklist", "gerenciar blacklist")]
public class BlacklistInteractionModule(
    IBlacklistService service,
    IBlacklistResponseHandler responseHandler,
    ILogger<BlacklistInteractionModule> _logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("lista", "lista todos os usuarios inseridos na blacklist")]
    public async Task ListAll()
    {
        _logger.LogCommandExecution(
            "Blacklist", Context.User.Username, Context.Channel.Name, nameof(ListAll));

        await DeferAsync();
        await service.ListAllAsync(Context.Interaction);
    }

    [SlashCommand("remover", "remove um usuario da blacklist")]
    public async Task RemoveFrom(SocketUser usuario)
    {
        _logger.LogCommandExecution(
            "Blacklist", Context.User.Username, Context.Channel.Name, nameof(RemoveFrom));

        await DeferAsync();
        if (!await UserHasPermission()) return;
        if (await IsBackupInProgress()) return;

        await service.RemoveFromAsync(Context.Interaction, usuario);
    }

    [SlashCommand("adicionar", "adiciona um usuario na blacklist")]
    public async Task AddToBlacklist(SocketUser usuario)
    {
        _logger.LogCommandExecution(
            "Blacklist", Context.User.Username, Context.Channel.Name, nameof(AddToBlacklist));

        await DeferAsync();
        if (!await UserHasPermission()) return;
        if (await IsBackupInProgress()) return;

        await service.AddToBlacklistAsync(Context.Interaction, usuario);
    }

    private async Task<bool> UserHasPermission()
    {
        var user = Context.User as SocketGuildUser;
        if (user!.Roles.Any(x => x.Id == Program.MainAdminRoleId))
            return true;
        
        await responseHandler.SendNotRightPermissionAsync(Context);
        return false;
    }

    private async Task<bool> IsBackupInProgress()
    {
        if (!BackupInteractionModule.IsBackupInProgress) return false;

        _logger.LogInformation(
            "{service}: Can't alter blacklist while a backup process is running", BlacklistService.SERVICE_NAME);
        await responseHandler.SendNotAvailableAsync(Context.Interaction);
        return true;
    }
}