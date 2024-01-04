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
using OPZBot.Services.MessageBackup;

namespace OPZBot.Modules;

[Group("blacklist", "gerenciar blacklist")]
public class BlacklistInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IBlacklistService _service;
    private readonly IBlacklistResponseHandler _responseHandler;
    private readonly ILogger<BlacklistService> _logger;

    public BlacklistInteractionModule(
        IBlacklistService service,
        IBlacklistResponseHandler responseHandler,
        ILogger<BlacklistService> logger)
    {
        _service = service;
        _responseHandler = responseHandler;
        _logger = logger;
    }


    [SlashCommand("lista", "lista todos os usuarios inseridos na blacklist")]
    public async Task ListAll()
    {
        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(ListAll));

        await DeferAsync();
        await _service.ListAllAsync(Context.Interaction);
    }

    [SlashCommand("remover", "remove um usuario da blacklist")]
    public async Task RemoveFrom(SocketUser usuario)
    {
        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(RemoveFrom));

        await DeferAsync();
        if (!await UserHasPermission()) return;
        if (await IsBackupInProgress()) return;

        await _service.RemoveFromAsync(Context.Interaction, usuario);
    }

    [SlashCommand("adicionar", "adiciona um usuario na blacklist")]
    public async Task AddToBlacklist(SocketUser usuario)
    {
        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(AddToBlacklist));

        await DeferAsync();
        if (!await UserHasPermission()) return;
        if (await IsBackupInProgress()) return;

        await _service.AddToBlacklistAsync(Context.Interaction, usuario);
    }

    private async Task<bool> UserHasPermission()
    {
        var user = Context.User as SocketGuildUser;
        if (user!.Roles.Any(x => x.Id == Program.MainAdminRoleId))
            return true;

        _logger.LogInformation(
            "{service}: {user} does not have the right permission", BlacklistService.SERVICE_NAME,
            Context.User.Username);
        await _responseHandler.SendNotRightPermissionAsync(Context);
        return false;
    }

    private async Task<bool> IsBackupInProgress()
    {
        if (!BackupInteractionModule.IsBackupInProgress) return false;

        _logger.LogInformation(
            "{service}: Can't alter blacklist while a backup process is running", BlacklistService.SERVICE_NAME);
        await _responseHandler.SendNotAvailableAsync(Context.Interaction);
        return true;
    }
}