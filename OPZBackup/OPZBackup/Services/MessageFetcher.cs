// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.WebSocket;

namespace OPZBackup.Services;

public class MessageFetcher : IMessageFetcher
{
    public async Task<IEnumerable<IMessage>> FetchAsync(ISocketMessageChannel channel)
    {
        return await channel.GetMessagesAsync(500).FlattenAsync();
    }

    public Task<IEnumerable<IMessage>> FetchAsync(ISocketMessageChannel channel, ulong startFrom)
    {
        return channel.GetMessagesAsync(startFrom, Direction.Before, 500).FlattenAsync();
    }
}