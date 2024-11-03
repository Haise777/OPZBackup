using Discord;
using Discord.WebSocket;

namespace OPZBackup.Services.Utils;

public class MessageFetcher
{
    public async Task<IEnumerable<IMessage>> FetchAsync(ISocketMessageChannel channel)
    {
        return await channel.GetMessagesAsync(AppInfo.MaxMessagesPerBatch).FlattenAsync();
    }

    public Task<IEnumerable<IMessage>> FetchAsync(ISocketMessageChannel channel, ulong startFrom)
    {
        return channel.GetMessagesAsync(startFrom, Direction.Before, AppInfo.MaxMessagesPerBatch).FlattenAsync();
    }
}