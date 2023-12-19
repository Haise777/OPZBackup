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
    public Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel)
    {
        return channel.GetMessagesAsync(10).FlattenAsync();
    }

    public Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel, ulong startFrom)
    {
        return channel.GetMessagesAsync(startFrom, Direction.Before, 10).FlattenAsync();
    }
}