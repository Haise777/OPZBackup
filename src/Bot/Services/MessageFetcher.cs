using Discord;
using Discord.WebSocket;

namespace OPZBot;

public class MessageFetcher
{
    public async Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel)
    {
        return await channel.GetMessagesAsync(10).FlattenAsync();
    }
    
    public async Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel, ulong startFrom)
    {
        return await channel.GetMessagesAsync(startFrom, Direction.Before, 10).FlattenAsync();
    }
}