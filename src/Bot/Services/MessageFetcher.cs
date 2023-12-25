// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.WebSocket;

namespace OPZBot.Services;

public class MessageFetcher : IMessageFetcher
{
    public async Task<IEnumerable<IMessage>> FetchAsync(ISocketMessageChannel channel)
    {
        var x = await channel.GetMessagesAsync(10).FlattenAsync();
        return x;
    }

    public async Task<IEnumerable<IMessage>> FetchAsync(ISocketMessageChannel channel, ulong startFrom)
    {
        return await channel.GetMessagesAsync(startFrom, Direction.Before, limit: 10).FlattenAsync();
    }
}