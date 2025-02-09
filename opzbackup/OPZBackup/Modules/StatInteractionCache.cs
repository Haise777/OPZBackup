using Discord.Interactions;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;

namespace OPZBackup.Modules;

public class StatInteractionCache
{
    private Dictionary<ulong, DetailedUserInteractionState> _detailedUserInteractionStates = new();
    private Dictionary<ulong, UsersInteractionState> _usersInteractionStates = new();
    private Dictionary<ulong, ChannelsInteractionState> _channelsInteractionStates = new();
    private Dictionary<ulong, DetailedChannelInteractionState> _detailedChannelInteractionStates = new();
    


    public DetailedUserInteractionState AddInteraction(SocketInteractionContext interaction, UserStats userStats)
    {
        var interactionState = new DetailedUserInteractionState(interaction, userStats);

        if (_detailedUserInteractionStates.Count > 10)
            _detailedUserInteractionStates.Remove(_detailedUserInteractionStates.Keys.Last());

        // if (_channelsContext.ContainsKey(interaction.Channel.Id))
        //     _channelsContext[interaction.Channel.Id] = interactionState;
        // else
        //     _channelsContext.Add(interaction.Channel.Id, interactionState);

        _detailedUserInteractionStates[interaction.Channel.Id] = interactionState;
        
        return interactionState;
    }
    
    public UsersInteractionState AddInteraction(SocketInteractionContext interaction, IEnumerable<User> users)
    {
        var interactionState = new UsersInteractionState(interaction, users);

        if (_usersInteractionStates.Count > 10)
            _usersInteractionStates.Remove(_usersInteractionStates.Keys.Last());

        // if (_channelsContext.ContainsKey(interaction.Channel.Id))
        //     _channelsContext[interaction.Channel.Id] = interactionState;
        // else
        //     _channelsContext.Add(interaction.Channel.Id, interactionState);

        _usersInteractionStates[interaction.Channel.Id] = interactionState;
        
        return interactionState;
    }
    
    public ChannelsInteractionState AddInteraction(SocketInteractionContext interaction, IEnumerable<Channel> channels)
    {
        var interactionState = new ChannelsInteractionState(interaction, channels);

        if (_channelsInteractionStates.Count > 10)
            _channelsInteractionStates.Remove(_channelsInteractionStates.Keys.Last());

        // if (_channelsContext.ContainsKey(interaction.Channel.Id))
        //     _channelsContext[interaction.Channel.Id] = interactionState;
        // else
        //     _channelsContext.Add(interaction.Channel.Id, interactionState);

        _channelsInteractionStates[interaction.Channel.Id] = interactionState;
        
        return interactionState;
    }
    
    public DetailedChannelInteractionState AddInteraction(SocketInteractionContext interaction, ChannelStats channelStats)
    {
        var interactionState = new DetailedChannelInteractionState(interaction, channelStats);

        if (_detailedChannelInteractionStates.Count > 10)
            _detailedChannelInteractionStates.Remove(_detailedChannelInteractionStates.Keys.Last());

        // if (_channelsContext.ContainsKey(interaction.Channel.Id))
        //     _channelsContext[interaction.Channel.Id] = interactionState;
        // else
        //     _channelsContext.Add(interaction.Channel.Id, interactionState);

        _detailedChannelInteractionStates[interaction.Channel.Id] = interactionState;
        
        return interactionState;
    }

    public DetailedUserInteractionState? GetDetailedUserInteraction(ulong channelId)
    {
        return _detailedUserInteractionStates.TryGetValue(channelId, out var interaction) ? interaction : null;
    }
    
    public UsersInteractionState? GetUsersInteraction(ulong channelId)
    {
        return _usersInteractionStates.TryGetValue(channelId, out var interaction) ? interaction : null;
    }
    
    public ChannelsInteractionState? GetChannelsInteraction(ulong channelId)
    {
        return _channelsInteractionStates.TryGetValue(channelId, out var interaction) ? interaction : null;
    }
    
    public DetailedChannelInteractionState? GetDetailedChannelInteraction(ulong channelId)
    {
        return _detailedChannelInteractionStates.TryGetValue(channelId, out var interaction) ? interaction : null;
    }
}

public class DetailedChannelInteractionState
{
    public readonly SocketInteractionContext interactionContext;
    public readonly ChannelStats channelStats;
    public int currentPage;
    
    public DetailedChannelInteractionState(SocketInteractionContext interactionContext, ChannelStats channelStats)
    {
        this.channelStats = channelStats;
        this.interactionContext = interactionContext;
    }
}

public class UsersInteractionState
{
    public readonly SocketInteractionContext interactionContext;
    public readonly User[][] users;
    public int currentPage;
    
    public UsersInteractionState(SocketInteractionContext interactionContext, IEnumerable<User> users)
    {
        this.users = users.Chunk(5).ToArray();
        this.interactionContext = interactionContext;
    }
}

public class ChannelsInteractionState
{
    public readonly SocketInteractionContext interactionContext;
    public readonly Channel[][] channels;
    public int currentPage;
    
    public ChannelsInteractionState(SocketInteractionContext interactionContext, IEnumerable<Channel> channels)
    {
        this.channels = channels.Chunk(10).ToArray();
        this.interactionContext = interactionContext;
    }
}

public class DetailedUserInteractionState
{
    public string selectBoxOption = "table1";
    public readonly UserStats userStats;
    public SocketInteractionContext Interaction { get; private set; }

    public Dictionary<string, int> currentTablePage = new()
    {
        ["table1"] = 0,
        ["table2"] = 0
    };

    public DetailedUserInteractionState(SocketInteractionContext interaction, UserStats userStats)
    {
        this.userStats = userStats;
        Interaction = interaction;
    }
}