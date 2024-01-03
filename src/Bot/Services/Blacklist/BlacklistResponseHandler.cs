// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.WebSocket;
using OPZBot.Utilities;

namespace OPZBot.Services.Blacklist;

public class BlacklistResponseHandler : BaseResponseHandler, IBlacklistResponseHandler
{
    public async Task SendNotAvailableAsync(SocketInteraction interaction)
    {
        await SendTempMessage(interaction, "*Não é possivel alterar a blacklist enquanto há um backup em andamento*");
    }

    public async Task SendNoUserInBlacklistAsync(SocketInteraction interaction)
    {
        await SendTempMessage(interaction, "*Não há nenhum usuario atualmente na blacklist*");
    }

    public async Task SendAllUsersAsync(SocketInteraction interaction, IEnumerable<string> blacklisteds)
    {
        await interaction.ModifyOriginalResponseAsync(m =>
        {
            foreach (var blacklisted in blacklisteds) m.Content += $"- {blacklisted}\n";
        });
    }

    public async Task SendUserNotExistsAsync(SocketInteraction interaction)
    {
        await SendTempMessage(interaction, "*Usuario não existente na blacklist*");
    }

    public async Task SendInteractionErrorAsync(SocketInteraction interaction)
    {
        await SendTempMessage(interaction, "*Ocorreu um erro ao tentar alterar a blacklist*");
    }

    public async Task SendUserRemovedAsync(SocketInteraction interaction, string username)
    {
        await interaction.ModifyOriginalResponseAsync(m =>
            m.Content = $"*{username} foi removido da blacklist*");
    }

    public async Task SendUserAlreadyAddedAsync(SocketInteraction interaction)
    {
        await SendTempMessage(interaction, "*Usuario já adicionado à blacklist*");
    }

    public async Task SendUserAddedAsync(SocketInteraction interaction, string username)
    {
        await interaction.ModifyOriginalResponseAsync(m =>
            m.Content = $"*{username} foi adicionado(a) à blacklist*");
    }

    //Sends a temporary auto-deleting message to the API
    private async Task SendTempMessage(SocketInteraction interaction, string message)
    {
        await interaction.ModifyOriginalResponseAsync(m => m.Content = message);
        DelayedDeleteInteraction(interaction);
    }
}