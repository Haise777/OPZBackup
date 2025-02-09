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

    public async Task SendChannelsStatsAsync(ChannelsInteractionState channelsInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateChannelsStats(channelsInteractionState.channels);
        
        var noAdvance = channelsInteractionState.currentPage + 1 >= channelsInteractionState.channels.Length;
        var noBack = channelsInteractionState.currentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId("advance-page2")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId("return-page2")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await interactionContext.Interaction.FollowupAsync(embed: embed, components: builder.Build());
    }
    
    public async Task SendUpdateChannelsStatsAsync(ChannelsInteractionState channelsInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateChannelsStats(channelsInteractionState.channels, channelsInteractionState.currentPage);
        
        var noAdvance = channelsInteractionState.currentPage + 1 >= channelsInteractionState.channels.Length;
        var noBack = channelsInteractionState.currentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId("advance-page2")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId("return-page2")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await channelsInteractionState.interactionContext.Interaction.ModifyOriginalResponseAsync(r =>
        {
            r.Embed = embed;
            r.Components = builder.Build();
        });
    }

    public async Task SendDetailedChannelStatsAsync(DetailedChannelInteractionState detailedChannelInteraction, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedChannelStats(detailedChannelInteraction.channelStats);
        
        var noAdvance = detailedChannelInteraction.currentPage + 1 >= detailedChannelInteraction.channelStats.users.Length;
        var noBack = detailedChannelInteraction.currentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId("advance-page3")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId("return-page3")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await interactionContext.Interaction.FollowupAsync(embed: embed, components: builder.Build());
    }
    
    public async Task SendUpdatedDetailedChannelStatsAsync(DetailedChannelInteractionState detailedChannelInteraction, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedChannelStats(detailedChannelInteraction.channelStats, detailedChannelInteraction.currentPage);
        
        var noAdvance = detailedChannelInteraction.currentPage + 1 >= detailedChannelInteraction.channelStats.users.Length;
        var noBack = detailedChannelInteraction.currentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId("advance-page3")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId("return-page3")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await detailedChannelInteraction.interactionContext.Interaction.ModifyOriginalResponseAsync(r =>
        {
            r.Embed = embed;
            r.Components = builder.Build();
        });
    }

    public async Task SendUsersStatsAsync(UsersInteractionState usersInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateUsersStats(usersInteractionState.users);
        
        var noAdvance = usersInteractionState.currentPage + 1 >= usersInteractionState.users.Length;
        var noBack = usersInteractionState.currentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId("advance-page1")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId("return-page1")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await interactionContext.Interaction.FollowupAsync(embed: embed, components: builder.Build());
    }
    
    public async Task SendUpdateUsersStatsAsync(UsersInteractionState usersInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateUsersStats(usersInteractionState.users, usersInteractionState.currentPage);
        
        var noAdvance = usersInteractionState.currentPage + 1 >= usersInteractionState.users.Length;
        var noBack = usersInteractionState.currentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId("advance-page1")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId("return-page1")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await usersInteractionState.interactionContext.Interaction.ModifyOriginalResponseAsync(r =>
        {
            r.Embed = embed;
            r.Components = builder.Build();
        });
    }

    public async Task SendDetailedUserStatsAsync(DetailedUserInteractionState userInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedUserStats(userInteractionState.userStats);

        var tableLenght = 0;

        if (userInteractionState.selectBoxOption == "table1")
            tableLenght = userInteractionState.userStats.MostCommonWords.Length;
        else if (userInteractionState.selectBoxOption == "table2")
            tableLenght = userInteractionState.userStats.NumberOfMentions.Length;

        var noAdvance = userInteractionState.currentTablePage[userInteractionState.selectBoxOption] + 1 >= tableLenght;
        var noBack = userInteractionState.currentTablePage[userInteractionState.selectBoxOption] == 0;
        
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

    public async Task SendUpdatedUserStatsAsync(DetailedUserInteractionState detailedUserInteractionState,
        SocketInteractionContext context)
    {
        var embed = _embedResponseFactory.CreateDetailedUserStats(detailedUserInteractionState.userStats,
            detailedUserInteractionState.currentTablePage["table1"], detailedUserInteractionState.currentTablePage["table2"]);

        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId("menu-1")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Option A", "table1", "Option A is lying!")
            .AddOption("Option B", "table2", "Option B is telling the truth!");

        var tableLenght = 0;

        if (detailedUserInteractionState.selectBoxOption == "table1")
            tableLenght = detailedUserInteractionState.userStats.MostCommonWords.Length;
        else if (detailedUserInteractionState.selectBoxOption == "table2")
            tableLenght = detailedUserInteractionState.userStats.NumberOfMentions.Length;

        var noAdvance = detailedUserInteractionState.currentTablePage[detailedUserInteractionState.selectBoxOption] + 1 >= tableLenght - 1;
        var noBack = detailedUserInteractionState.currentTablePage[detailedUserInteractionState.selectBoxOption] == 0;
        
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