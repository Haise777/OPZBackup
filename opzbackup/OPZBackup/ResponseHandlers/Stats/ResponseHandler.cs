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
    //TODO: Heavily refactor this to reduce code duplication across all methods

    public async Task SendChannelsStatsAsync(InteractionState<Channel[][]> channelsInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateChannelsStats(channelsInteractionState.data);
        
        var noAdvance = channelsInteractionState.CurrentPage + 1 >= channelsInteractionState.data.Length;
        var noBack = channelsInteractionState.CurrentPage == 0;
        
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
    
    public async Task SendUpdateChannelsStatsAsync(InteractionState<Channel[][]> channelsInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateChannelsStats(channelsInteractionState.data, channelsInteractionState.CurrentPage);
        
        var noAdvance = channelsInteractionState.CurrentPage + 1 >= channelsInteractionState.data.Length;
        var noBack = channelsInteractionState.CurrentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId($"channels-advancepage-{channelsInteractionState.interactionContext.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId($"channels-returnpage-{channelsInteractionState.interactionContext.Interaction.Id}")
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

    public async Task SendDetailedChannelStatsAsync(InteractionState<ChannelStats> detailedChannelInteraction, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedChannelStats(detailedChannelInteraction.data);
        
        var noAdvance = detailedChannelInteraction.CurrentPage + 1 >= detailedChannelInteraction.data.users.Length;
        var noBack = detailedChannelInteraction.CurrentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId($"detailedchannel-advancepage-{interactionContext.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId($"detailedchannel-returnpage-{interactionContext.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await interactionContext.Interaction.FollowupAsync(embed: embed, components: builder.Build());
    }
    
    public async Task SendUpdatedDetailedChannelStatsAsync(InteractionState<ChannelStats> detailedChannelInteraction, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedChannelStats(detailedChannelInteraction.data, detailedChannelInteraction.CurrentPage);
        
        var noAdvance = detailedChannelInteraction.CurrentPage + 1 >= detailedChannelInteraction.data.users.Length;
        var noBack = detailedChannelInteraction.CurrentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId($"detailedchannel-advancepage-{detailedChannelInteraction.interactionContext.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId($"detailedchannel-returnpage-{detailedChannelInteraction.interactionContext.Interaction.Id}")
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

    public async Task SendUsersStatsAsync(InteractionState<User[][]> usersInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateUsersStats(usersInteractionState.data);
        
        var noAdvance = usersInteractionState.CurrentPage + 1 >= usersInteractionState.data.Length;
        var noBack = usersInteractionState.CurrentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId($"users-advancepage-{usersInteractionState.interactionContext.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId($"users-returnpage-{usersInteractionState.interactionContext.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await interactionContext.Interaction.FollowupAsync(embed: embed, components: builder.Build());
    }
    
    public async Task SendUpdateUsersStatsAsync(InteractionState<User[][]> usersInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateUsersStats(usersInteractionState.data, usersInteractionState.CurrentPage);
        
        var noAdvance = usersInteractionState.CurrentPage + 1 >= usersInteractionState.data.Length;
        var noBack = usersInteractionState.CurrentPage == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId($"users-advancepage-{usersInteractionState.interactionContext.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId($"users-returnpage-{usersInteractionState.interactionContext.Interaction.Id}")
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

    public async Task SendDetailedUserStatsAsync(MultiTableInteractionState<UserStatsWithUsernames> userInteractionState, SocketInteractionContext interactionContext)
    {
        var embed = _embedResponseFactory.CreateDetailedUserStats(userInteractionState.data, userInteractionState.selectBoxOption);

        var tableLenght = 0;

        if (userInteractionState.selectBoxOption == "table1")
            tableLenght = userInteractionState.data.MostCommonWords.Length;
        else if (userInteractionState.selectBoxOption == "table2")
            tableLenght = userInteractionState.data.NumberOfMentions.Length;

        var noAdvance = userInteractionState.currentTablePage[userInteractionState.selectBoxOption] >= tableLenght - 1;
        var noBack = userInteractionState.currentTablePage[userInteractionState.selectBoxOption] == 0;
        
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId($"detaileduser-tableswitch-{userInteractionState.Interaction.Interaction.Id}")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Option A", "table1", "Option A is lying!")
            .AddOption("Option B", "table2", "Option B is telling the truth!")
            .AddOption("Option C", "table3", "Option C is telling the truth!");

        var buttonBuilder = new ButtonBuilder()
            .WithCustomId($"detaileduser-advancepage-{userInteractionState.Interaction.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId($"detaileduser-returnpage-{userInteractionState.Interaction.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Voltar")
            .WithDisabled(noBack);

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder)
            .WithButton(buttonBuilder1)
            .WithButton(buttonBuilder);

        await interactionContext.Interaction.FollowupAsync(embed: embed, components: builder.Build());
    }

    public async Task SendUpdatedUserStatsAsync(MultiTableInteractionState<UserStatsWithUsernames> detailedUserInteractionState,
        SocketInteractionContext context)
    {
        var embed = _embedResponseFactory.CreateDetailedUserStats(detailedUserInteractionState.data,
            detailedUserInteractionState.selectBoxOption,
            detailedUserInteractionState.currentTablePage[detailedUserInteractionState.selectBoxOption]);

        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId($"detaileduser-tableswitch-{detailedUserInteractionState.Interaction.Interaction.Id}")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Option A", "table1", "Option A is lying!")
            .AddOption("Option B", "table2", "Option B is telling the truth!")
            .AddOption("Option C", "table3", "Option C is telling the truth!");

        var tableLenght = 0;

        if (detailedUserInteractionState.selectBoxOption == "table1")
            tableLenght = detailedUserInteractionState.data.MostCommonWords.Length;
        else if (detailedUserInteractionState.selectBoxOption == "table2")
            tableLenght = detailedUserInteractionState.data.NumberOfMentions.Length;

        var noAdvance = detailedUserInteractionState.currentTablePage[detailedUserInteractionState.selectBoxOption] >= tableLenght - 1;
        var noBack = detailedUserInteractionState.currentTablePage[detailedUserInteractionState.selectBoxOption] == 0;
        
        var buttonBuilder = new ButtonBuilder()
            .WithCustomId($"detaileduser-advancepage-{detailedUserInteractionState.Interaction.Interaction.Id}")
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Avançar")
            .WithDisabled(noAdvance);

        var buttonBuilder1 = new ButtonBuilder()
            .WithCustomId($"detaileduser-returnpage-{detailedUserInteractionState.Interaction.Interaction.Id}")
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