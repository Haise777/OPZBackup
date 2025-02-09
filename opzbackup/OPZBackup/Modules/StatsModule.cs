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
        // _statInteractionCache.AddInteraction(Context);

        await _responseHandler.SendChannelsStatsAsync(channels, Context);
    }

    [SlashCommand("canal-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailChannelStats()
    {
        await Context.Interaction.DeferAsync();

        var channelStats = await _statsService.GetInDetailChannelStats(Context.Channel.Id);
        // _statInteractionCache.AddInteraction(Context);

        await _responseHandler.SendDetailedChannelStatsAsync(channelStats, Context);
    }

    [SlashCommand("usuario-listar", "efetuar backup deste canal")]
    public async Task ListAllUsersStats()
    {
        await Context.Interaction.DeferAsync();

        var users = await _statsService.ListAllUsersStats();
        // _statInteractionCache.AddInteraction(Context);

        await _responseHandler.SendUsersStatsAsync(users, Context);
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
        var interactionState = _statInteractionCache.GetInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.selectBoxOption = choice;
        await _responseHandler.SendUpdatedUserStatsAsync(interactionState, interactionState.Interaction);
    }

    [ComponentInteraction("advance-page", true)]
    public async Task AdvancePage()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentTablePage[interactionState.selectBoxOption]++;
        // await Context.Interaction.RespondAsync(interactionState.currentPage.ToString());
        
        await _responseHandler.SendUpdatedUserStatsAsync(interactionState, interactionState.Interaction);
    }

    [ComponentInteraction("return-page", true)]
    public async Task ReturnPage()
    {
        await Context.Interaction.DeferAsync();
        var interactionState = _statInteractionCache.GetInteraction(Context.Channel.Id);
        if (interactionState is null)
            return;

        interactionState.currentTablePage[interactionState.selectBoxOption]--;
        
        await _responseHandler.SendUpdatedUserStatsAsync(interactionState, interactionState.Interaction);
    }
}