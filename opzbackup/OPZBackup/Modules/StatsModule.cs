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
        var interaction = _statInteractionCache.AddInteraction(Context, channels, _responseHandler);

        await _responseHandler.SendChannelsStatsAsync(interaction, Context);
    }
    
    [SlashCommand("canal-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailChannelStats()
    {
        await Context.Interaction.DeferAsync();

        var channelStats = await _statsService.GetInDetailChannelStats(Context.Channel.Id);
        var interaction = _statInteractionCache.AddInteraction(Context, channelStats, _responseHandler);

        await _responseHandler.SendDetailedChannelStatsAsync(interaction, Context);
    }

    [SlashCommand("usuario-listar", "efetuar backup deste canal")]
    public async Task ListAllUsersStats()
    {
        await Context.Interaction.DeferAsync();

        var users = await _statsService.ListAllUsersStats();
        var interaction = _statInteractionCache.AddInteraction(Context, users, _responseHandler);

        await _responseHandler.SendUsersStatsAsync(interaction, Context);
    }

    [SlashCommand("usuario-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailUserStats()
    {
        await Context.Interaction.DeferAsync();

        var userStats = await _statsService.GetInDetailUserStats(Context.User.Id);
        var interaction = _statInteractionCache.AddInteraction(Context, userStats, _responseHandler);
        
        await _responseHandler.SendDetailedUserStatsAsync(interaction, Context);
    }
}