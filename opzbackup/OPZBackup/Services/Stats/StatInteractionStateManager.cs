using Discord.Interactions;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.ResponseHandlers;
using OPZBackup.ResponseHandlers.Stats;

namespace OPZBackup.Services.Stats;

public class StatInteractionStateManager
{
    //TODO: Add a nicer and more centralized way to track how much States are still active and what to do about them
    // And also 15 minutes decay for them, since the SocketInteractionContext only lives up to 15 minutes.
    private Dictionary<ulong, MultiTableInteractionState<UserStatsWithUsernames>> _detailedUserInteractionStates = new();
    private Dictionary<ulong, InteractionState<User[][]>> _usersInteractionStates = new();
    private Dictionary<ulong, InteractionState<Channel[][]>> _channelsInteractionStates = new();
    private Dictionary<ulong, InteractionState<ChannelStats>> _detailedChannelInteractionStates = new();

    public MultiTableInteractionState<UserStatsWithUsernames> AddInteraction(SocketInteractionContext interaction,
        UserStatsWithUsernames userStats, ResponseHandler responseHandler)
    {
        var tablePageDictionary = new Dictionary<string, int>()
        {
            ["table1"] = 0,
            ["table2"] = 0,
            ["table3"] = 0,
        };
        
        var interactionState = new MultiTableInteractionState<UserStatsWithUsernames>(interaction, userStats, tablePageDictionary);

        if (_detailedUserInteractionStates.Count > 10)
            _detailedUserInteractionStates.Remove(_detailedUserInteractionStates.Keys.Last());

        _detailedUserInteractionStates[interaction.Interaction.Id] = interactionState;

        return interactionState;
    }

    public InteractionState<User[][]> AddInteraction(SocketInteractionContext interaction,
        User[][] users)
    {
        var interactionState = new InteractionState<User[][]>(interaction, users);

        if (_usersInteractionStates.Count > 10)
            _usersInteractionStates.Remove(_usersInteractionStates.Keys.Last());

        _usersInteractionStates[interaction.Interaction.Id] = interactionState;

        return interactionState;
    }

    public InteractionState<Channel[][]> AddInteraction(SocketInteractionContext interaction,
        Channel[][] channels)
    {
        var interactionState = new InteractionState<Channel[][]>(interaction, channels);

        if (_channelsInteractionStates.Count > 10)
            _channelsInteractionStates.Remove(_channelsInteractionStates.Keys.Last());

        _channelsInteractionStates[interaction.Interaction.Id] = interactionState;

        return interactionState;
    }

    public InteractionState<ChannelStats> AddInteraction(SocketInteractionContext interaction,
        ChannelStats channelStats)
    {
        var interactionState = new InteractionState<ChannelStats>(interaction, channelStats);

        if (_detailedChannelInteractionStates.Count > 10)
            _detailedChannelInteractionStates.Remove(_detailedChannelInteractionStates.Keys.Last());

        _detailedChannelInteractionStates[interaction.Interaction.Id] = interactionState;

        return interactionState;
    }

    public MultiTableInteractionState<UserStatsWithUsernames>? GetDetailedUserInteraction(ulong channelId)
    {
        return _detailedUserInteractionStates.TryGetValue(channelId, out var interaction) ? interaction : null;
    }

    public InteractionState<User[][]>? GetUsersInteraction(ulong channelId)
    {
        return _usersInteractionStates.TryGetValue(channelId, out var interaction) ? interaction : null;
    }

    public InteractionState<Channel[][]>? GetChannelsInteraction(ulong channelId)
    {
        return _channelsInteractionStates.TryGetValue(channelId, out var interaction) ? interaction : null;
    }

    public InteractionState<ChannelStats>? GetDetailedChannelInteraction(ulong channelId)
    {
        return _detailedChannelInteractionStates.TryGetValue(channelId, out var interaction) ? interaction : null;
    }
}