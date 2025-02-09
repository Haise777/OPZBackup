using Discord.Interactions;
using OPZBackup.Data.Dto;

namespace OPZBackup.Modules;

public class StatInteractionCache
{
    private Dictionary<ulong, StatInteractionState> _channelsContext = new();

    public StatInteractionState AddInteraction(SocketInteractionContext interaction, UserStats userStats)
    {
        var interactionState = new StatInteractionState(interaction, userStats);

        if (_channelsContext.Count > 10)
            _channelsContext.Remove(_channelsContext.Keys.Last());

        // if (_channelsContext.ContainsKey(interaction.Channel.Id))
        //     _channelsContext[interaction.Channel.Id] = interactionState;
        // else
        //     _channelsContext.Add(interaction.Channel.Id, interactionState);

        _channelsContext[interaction.Channel.Id] = interactionState;
        
        return interactionState;
    }

    public StatInteractionState? GetInteraction(ulong channelId)
    {
        return _channelsContext.TryGetValue(channelId, out var interaction) ? interaction : null;
    }
}

public class StatInteractionState
{
    public string selectBoxOption = "table1";
    public readonly UserStats userStats;
    public SocketInteractionContext Interaction { get; private set; }

    public Dictionary<string, int> currentTablePage = new()
    {
        ["table1"] = 0,
        ["table2"] = 0
    };

    public StatInteractionState(SocketInteractionContext interaction, UserStats userStats)
    {
        this.userStats = userStats;
        Interaction = interaction;
    }
}