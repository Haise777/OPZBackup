using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;

namespace OPZBackup.ResponseHandlers.Stats;

public class ResponseHandler
{
    private readonly SocketInteractionContext _interactionContext;
    private readonly EmbedResponseFactory _embedResponseFactory;

    public ResponseHandler(SocketInteractionContext interactionContext, EmbedResponseFactory embedResponseFactory)
    {
        _interactionContext = interactionContext;
        _embedResponseFactory = embedResponseFactory;
    }

    public async Task SendChannelsStatsAsync(IEnumerable<Channel> channels)
    {
        var embed = _embedResponseFactory.CreateChannelsStats(channels);
        await _interactionContext.Interaction.FollowupAsync(embed: embed);
    }

    public async Task SendDetailedChannelStatsAsync(ChannelStats channelStats)
    {
        var embed = _embedResponseFactory.CreateDetailedChannelStats(channelStats);
        await _interactionContext.Interaction.FollowupAsync(embed: embed);
    }

    public async Task SendUsersStatsAsync(IEnumerable<User> users)
    {
        var embed = _embedResponseFactory.CreateUsersStats(users);
        await _interactionContext.Interaction.FollowupAsync(embed: embed);
    }

    public async Task SendDetailedUserStatsAsync(UserStats userStats)
    {
        var embed = _embedResponseFactory.CreateDetailedUserStats(userStats);
        await _interactionContext.Interaction.FollowupAsync(embed: embed);
    }
}