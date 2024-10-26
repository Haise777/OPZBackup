// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.WebSocket;

namespace OPZBackup.Services.Blacklist;

public interface IBlacklistResponseHandler : IResponseHandler
{
    Task SendNotAvailableAsync(SocketInteraction interaction);
    Task SendNoUserInBlacklistAsync(SocketInteraction interaction);
    Task SendAllUsersAsync(SocketInteraction interaction, IEnumerable<string> blacklisteds);
    Task SendUserNotExistsAsync(SocketInteraction interaction);
    Task SendInteractionErrorAsync(SocketInteraction interaction);
    Task SendUserRemovedAsync(SocketInteraction interaction, string username);
    Task SendUserAlreadyAddedAsync(SocketInteraction interaction);
    Task SendUserAddedAsync(SocketInteraction interaction, string username);
}