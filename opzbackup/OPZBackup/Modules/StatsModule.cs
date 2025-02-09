using Discord.Commands;
using Discord.Interactions;
using OPZBackup.Data.Dto;
using OPZBackup.ResponseHandlers.Stats;
using OPZBackup.Services.Stats;

namespace OPZBackup.Modules;

[Discord.Interactions.Group("stats", "utilizar a função de stats")]
public class StatsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly StatsService _statsService;
    private readonly ResponseHandler _responseHandler;
    private readonly StatInteractionCache _statInteractionCache;

    public StatsModule(StatsService statsService, ResponseHandler responseHandler,
        StatInteractionCache statInteractionCache)
    {
        _statsService = statsService;
        _responseHandler = responseHandler;
        _statInteractionCache = statInteractionCache;
    }

    [SlashCommand("canal-listar", "efetuar backup deste canal")]
    public async Task ListAllChannelStats()
    {
        await Context.Interaction.DeferAsync();

        var channels = await _statsService.ListAllChannelStats();
        var interaction = _statInteractionCache.AddInteraction(Context, channels);

        await _responseHandler.SendChannelsStatsAsync(interaction, Context);
    }
    
    [ComponentInteraction("advance-page2", true)]
    public async Task AdvancePage2()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetChannelsInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentPage++;
        
        await _responseHandler.SendUpdateChannelsStatsAsync(interactionState, interactionState.interactionContext);
    }

    [ComponentInteraction("return-page2", true)]
    public async Task ReturnPage2()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetChannelsInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentPage--;
        
        await _responseHandler.SendUpdateChannelsStatsAsync(interactionState, interactionState.interactionContext);
    }

    [SlashCommand("canal-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailChannelStats()
    {
        await Context.Interaction.DeferAsync();

        var channelStats = await _statsService.GetInDetailChannelStats(Context.Channel.Id);
        var interaction = _statInteractionCache.AddInteraction(Context, channelStats);

        await _responseHandler.SendDetailedChannelStatsAsync(interaction, Context);
    }
    
    [ComponentInteraction("advance-page3", true)]
    public async Task AdvancePage3()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetDetailedChannelInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentPage++;
        
        await _responseHandler.SendUpdatedDetailedChannelStatsAsync(interactionState, interactionState.interactionContext);
    }

    [ComponentInteraction("return-page3", true)]
    public async Task ReturnPage3()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetDetailedChannelInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentPage--;
        
        await _responseHandler.SendUpdatedDetailedChannelStatsAsync(interactionState, interactionState.interactionContext);
    }

    [SlashCommand("usuario-listar", "efetuar backup deste canal")]
    public async Task ListAllUsersStats()
    {
        await Context.Interaction.DeferAsync();

        var users = await _statsService.ListAllUsersStats();
        var interaction = _statInteractionCache.AddInteraction(Context, users);

        await _responseHandler.SendUsersStatsAsync(interaction, Context);
    }

    [SlashCommand("usuario-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailUserStats()
    {
        await Context.Interaction.DeferAsync();

        var userStats = await _statsService.GetInDetailUserStats(Context.User.Id);
        var interaction = _statInteractionCache.AddInteraction(Context, userStats);
        
        await _responseHandler.SendDetailedUserStatsAsync(interaction, Context);
    }

    [ComponentInteraction("menu-1", true)]
    public async Task ListAllMenus(string choice)
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetDetailedUserInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.selectBoxOption = choice;
        await _responseHandler.SendUpdatedUserStatsAsync(interactionState, interactionState.Interaction);
    }

    [ComponentInteraction("advance-page", true)]
    public async Task AdvancePage()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetDetailedUserInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentTablePage[interactionState.selectBoxOption]++;
        
        await _responseHandler.SendUpdatedUserStatsAsync(interactionState, interactionState.Interaction);
    }

    [ComponentInteraction("return-page", true)]
    public async Task ReturnPage()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetDetailedUserInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentTablePage[interactionState.selectBoxOption]--;
        
        await _responseHandler.SendUpdatedUserStatsAsync(interactionState, interactionState.Interaction);
    }
    
    [ComponentInteraction("advance-page1", true)]
    public async Task AdvancePage1()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetUsersInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentPage++;
        
        await _responseHandler.SendUpdateUsersStatsAsync(interactionState, interactionState.interactionContext);
    }

    [ComponentInteraction("return-page1", true)]
    public async Task ReturnPage1()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetUsersInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentPage--;
        
        await _responseHandler.SendUpdateUsersStatsAsync(interactionState, interactionState.interactionContext);
    }
}