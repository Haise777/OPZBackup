using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.Modules;

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

    public async Task SendDetailedChannelStatsAsync(ChannelStats channelStats,
        SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedChannelStats(channelStats);
        await interactionContext.Interaction.FollowupAsync(embed: embed);
    }

    public async Task SendUsersStatsAsync(IEnumerable<User> users, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateUsersStats(users);
        await interactionContext.Interaction.FollowupAsync(embed: embed);
    }

    public async Task SendDetailedUserStatsAsync(StatInteractionState statInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedUserStats(statInteractionState.userStats);

        var tableLenght = 0;

        if (statInteractionState.selectBoxOption == "table1")
            tableLenght = statInteractionState.userStats.MostCommonWords.Length;
        else if (statInteractionState.selectBoxOption == "table2")
            tableLenght = statInteractionState.userStats.NumberOfMentions.Length;

        var noAdvance = statInteractionState.currentTablePage[statInteractionState.selectBoxOption] >= tableLenght;
        var noBack = statInteractionState.currentTablePage[statInteractionState.selectBoxOption] == 0;
        
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId("menu-1")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Option A", "table1", "Option A is lying!")
            .AddOption("Option B", "table2", "Option B is telling the truth!");

        var buttonBuilder = new ButtonBuilder()
            .WithCustomId("advance-page")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId("return-page")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder)
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await interactionContext.Interaction.FollowupAsync(embed: embed, components: builder.Build());
    }

    public async Task SendUpdatedUserStatsAsync(StatInteractionState statInteractionState,
        SocketInteractionContext context)
    {
        var embed = _embedResponseFactory.CreateDetailedUserStats(statInteractionState.userStats,
            statInteractionState.currentTablePage["table1"], statInteractionState.currentTablePage["table2"]);

        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId("menu-1")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Option A", "table1", "Option A is lying!")
            .AddOption("Option B", "table2", "Option B is telling the truth!");

        var tableLenght = 0;

        if (statInteractionState.selectBoxOption == "table1")
            tableLenght = statInteractionState.userStats.MostCommonWords.Length;
        else if (statInteractionState.selectBoxOption == "table2")
            tableLenght = statInteractionState.userStats.NumberOfMentions.Length;

        var noAdvance = statInteractionState.currentTablePage[statInteractionState.selectBoxOption] >= tableLenght - 1;
        var noBack = statInteractionState.currentTablePage[statInteractionState.selectBoxOption] == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId("advance-page")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId("return-page")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder)
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await context.Interaction.ModifyOriginalResponseAsync(r =>
        {
            r.Embed = embed;
            r.Components = builder.Build();
        });
    }
}