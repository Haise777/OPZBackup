using Discord;
using Discord.WebSocket;

namespace OPZBot.Services;

public interface IMessageFetcher
{
    Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel);
    Task<IEnumerable<IMessage>> Fetch(ISocketMessageChannel channel, ulong startFrom);
}