using Discord;
using Discord.WebSocket;

namespace OPZBot.Services;

public class MessageFetcher : IMessageFetcher
{
    public Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel) 
        => channel.GetMessagesAsync(10).FlattenAsync();

    public Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel, ulong startFrom) 
        => channel.GetMessagesAsync(startFrom, Direction.Before, 10).FlattenAsync();
}