// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Logging;
using Serilog;

namespace OPZBackup.Services;

public abstract class BaseResponseHandler : IResponseHandler
{
    public async Task SendNotRightPermissionAsync(SocketInteractionContext context)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Você não possui as permissões adequadas para este comando*");
        DelayedDeleteInteraction(context.Interaction);
    }

    protected void DelayedDeleteInteraction(SocketInteraction interaction)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(7000);
                await interaction.DeleteOriginalResponseAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                await LogFileWritter.LogError(ex, ex.Message);
            }
        });
    }
}