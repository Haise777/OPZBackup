using Discord;
using Discord.WebSocket;

namespace OPZBot;

public class MessageFetcher
{
    public async Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel, ulong startFrom)
    {
        if (startFrom == 0) return await channel.GetMessagesAsync(10).FlattenAsync();

        return await channel.GetMessagesAsync(startFrom, Direction.Before, 10).FlattenAsync();
    }
}