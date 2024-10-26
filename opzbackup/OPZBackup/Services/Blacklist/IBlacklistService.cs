// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.WebSocket;

namespace OPZBackup.Services.Blacklist;

public interface IBlacklistService
{
    Task ListAllAsync(SocketInteraction interaction);
    Task RemoveFromAsync(SocketInteraction interaction, SocketUser socketUser);
    Task AddToBlacklistAsync(SocketInteraction interaction, SocketUser socketUser);
}