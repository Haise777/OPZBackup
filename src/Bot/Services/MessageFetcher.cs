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