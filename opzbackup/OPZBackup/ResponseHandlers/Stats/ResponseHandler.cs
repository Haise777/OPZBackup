using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;

namespace OPZBackup.ResponseHandlers.Stats;

public class ResponseHandler
{
    private readonly EmbedResponseFactory _embedResponseFactory;

    public ResponseHandler(EmbedResponseFactory embedResponseFactory)
    {
        _embedResponseFactory = embedResponseFactory;
    }

    public async Task SendChannelsStatsAsync(IEnumerable<Channel> channels, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateChannelsStats(channels);
        await interactionContext.Interaction.FollowupAsync(embed: embed);
    }

    public async Task SendDetailedChannelStatsAsync(ChannelStats channelStats, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedChannelStats(channelStats);
        await interactionContext.Interaction.FollowupAsync(embed: embed);
    }

    public async Task SendUsersStatsAsync(IEnumerable<User> users, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateUsersStats(users);
        await interactionContext.Interaction.FollowupAsync(embed: embed);
    }

    public async Task SendDetailedUserStatsAsync(UserStats userStats, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedUserStats(userStats);
        await interactionContext.Interaction.FollowupAsync(embed: embed);
    }
}