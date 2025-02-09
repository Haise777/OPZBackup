using Discord.Interactions;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.ResponseHandlers.Stats;

namespace OPZBackup.Modules;

public class StatInteractionCache
{
    private Dictionary<ulong, DetailedUserInteractionState> _detailedUserInteractionStates = new();
    private Dictionary<ulong, UsersInteractionState> _usersInteractionStates = new();
    private Dictionary<ulong, ChannelsInteractionState> _channelsInteractionStates = new();
    private Dictionary<ulong, DetailedChannelInteractionState> _detailedChannelInteractionStates = new();
    


    public DetailedUserInteractionState AddInteraction(SocketInteractionContext interaction, UserStatsWithUsernames userStats, ResponseHandler responseHandler)
    {
        var interactionState = new DetailedUserInteractionState(interaction, userStats, responseHandler);

        if (_detailedUserInteractionStates.Count > 10)
            _detailedUserInteractionStates.Remove(_detailedUserInteractionStates.Keys.Last());

        _detailedUserInteractionStates[interaction.Interaction.Id] = interactionState;
        
        return interactionState;
    }
    
    public UsersInteractionState AddInteraction(SocketInteractionContext interaction, IEnumerable<User> users, ResponseHandler responseHandler)
    {
        var interactionState = new UsersInteractionState(interaction, users, responseHandler);

        if (_usersInteractionStates.Count > 10)
            _usersInteractionStates.Remove(_usersInteractionStates.Keys.Last());
        
        _usersInteractionStates[interaction.Interaction.Id] = interactionState;
        
        return interactionState;
    }
    
    public ChannelsInteractionState AddInteraction(SocketInteractionContext interaction, IEnumerable<Channel> channels, ResponseHandler responseHandler)
    {
        var interactionState = new ChannelsInteractionState(interaction, channels, responseHandler);

        if (_channelsInteractionStates.Count > 10)
            _channelsInteractionStates.Remove(_channelsInteractionStates.Keys.Last());
        
        _channelsInteractionStates[interaction.Interaction.Id] = interactionState;
        
        return interactionState;
    }
    
    public DetailedChannelInteractionState AddInteraction(SocketInteractionContext interaction, ChannelStats channelStats, ResponseHandler responseHandler)
    {
        var interactionState = new DetailedChannelInteractionState(interaction, channelStats, responseHandler);

        if (_detailedChannelInteractionStates.Count > 10)
            _detailedChannelInteractionStates.Remove(_detailedChannelInteractionStates.Keys.Last());

        _detailedChannelInteractionStates[interaction.Interaction.Id] = interactionState;
        
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
    public readonly ResponseHandler responseHandler;
    public int currentPage;
    
    public DetailedChannelInteractionState(SocketInteractionContext interactionContext, ChannelStats channelStats, ResponseHandler responseHandler)
    {
        this.channelStats = channelStats;
        this.responseHandler = responseHandler;
        this.interactionContext = interactionContext;
    }
    
    public async Task AdvancePage(SocketInteractionContext currentContext)
    {
        await currentContext.Interaction.DeferAsync();
        
        currentPage++;
        
        await responseHandler.SendUpdatedDetailedChannelStatsAsync(this, interactionContext);
    }
    
    public async Task ReturnPage(SocketInteractionContext currentContext)
    {
        await currentContext.Interaction.DeferAsync();

        currentPage--;
        
        await responseHandler.SendUpdatedDetailedChannelStatsAsync(this, interactionContext);
    }
}

public class UsersInteractionState
{
    public readonly SocketInteractionContext interactionContext;
    public readonly ResponseHandler responseHandler;
    public readonly User[][] users;
    public int currentPage;
    
    public UsersInteractionState(SocketInteractionContext interactionContext, IEnumerable<User> users, ResponseHandler responseHandler)
    {
        this.users = users.Chunk(5).ToArray();
        this.interactionContext = interactionContext;
        this.responseHandler = responseHandler;
    }
    
    public async Task AdvancePage(SocketInteractionContext currentContext)
    {
        await currentContext.Interaction.DeferAsync();
        
        currentPage++;
        
        await responseHandler.SendUpdateUsersStatsAsync(this, interactionContext);
    }
    
    public async Task ReturnPage(SocketInteractionContext currentContext)
    {
        await currentContext.Interaction.DeferAsync();

        currentPage--;
        
        await responseHandler.SendUpdateUsersStatsAsync(this, interactionContext);
    }
}

public class ChannelsInteractionState
{
    public readonly SocketInteractionContext interactionContext;
    public readonly ResponseHandler responseHandler;
    public readonly Channel[][] channels;
    public int currentPage;
    
    public ChannelsInteractionState(SocketInteractionContext interactionContext, IEnumerable<Channel> channels, ResponseHandler responseHandler)
    {
        this.channels = channels.Chunk(10).ToArray();
        this.interactionContext = interactionContext;
        this.responseHandler = responseHandler;
    }
    
    public async Task AdvancePage(SocketInteractionContext currentContext)
    {
        await currentContext.Interaction.DeferAsync();
        
        currentPage++;
        
        await responseHandler.SendUpdateChannelsStatsAsync(this, interactionContext);
    }
    
    public async Task ReturnPage(SocketInteractionContext currentContext)
    {
        await currentContext.Interaction.DeferAsync();

        currentPage--;
        
        await responseHandler.SendUpdateChannelsStatsAsync(this, interactionContext);
    }
}

public class DetailedUserInteractionState
{
    public string selectBoxOption = "table1";
    public readonly UserStatsWithUsernames userStats;
    public SocketInteractionContext Interaction { get; private set; }
    public readonly ResponseHandler responseHandler;

    public Dictionary<string, int> currentTablePage = new()
    {
        ["table1"] = 0,
        ["table2"] = 0,
        ["table3"] = 0
    };

    public DetailedUserInteractionState(SocketInteractionContext interaction, UserStatsWithUsernames userStats, ResponseHandler responseHandler)
    {
        this.userStats = userStats;
        this.responseHandler = responseHandler;
        Interaction = interaction;
    }
    
    public async Task SwitchTable(SocketInteractionContext context, string choice)
    {
        await context.Interaction.DeferAsync();

        selectBoxOption = choice;
        
        await responseHandler.SendUpdatedUserStatsAsync(this, Interaction);
    }
    
    public async Task AdvancePage(SocketInteractionContext context)
    {
        await context.Interaction.DeferAsync();

        currentTablePage[selectBoxOption]++;
        
        await responseHandler.SendUpdatedUserStatsAsync(this, Interaction);
    }
    
    public async Task ReturnPage(SocketInteractionContext context)
    {
        await context.Interaction.DeferAsync();

        currentTablePage[selectBoxOption]--;
        
        await responseHandler.SendUpdatedUserStatsAsync(this, Interaction);
    }
}