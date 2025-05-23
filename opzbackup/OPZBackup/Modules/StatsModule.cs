using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Data.Dto;
using OPZBackup.ResponseHandlers.Stats;
using OPZBackup.Services.Stats;

namespace OPZBackup.Modules;

[Discord.Interactions.Group("stats", "utilizar a função de stats")]
public class StatsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly StatsService _statsService;
    private readonly ResponseHandler _responseHandler;
    private readonly StatInteractionStateManager _statInteractionStateManager;
    
    //TODO: Add a better error handling across all depths beyond this layer

    public StatsModule(StatsService statsService, ResponseHandler responseHandler,
        StatInteractionStateManager statInteractionStateManager)
    {
        _statsService = statsService;
        _responseHandler = responseHandler;
        _statInteractionStateManager = statInteractionStateManager;
    }

    [SlashCommand("canal-listar", "efetuar backup deste canal")]
    public async Task ListAllChannelStats()
    {
//        await Context.Interaction.DeferAsync();

        var channels = await _statsService.ListAllChannelStats();
        var chunkedChannels = channels.Chunk(10).ToArray();
        var interaction = _statInteractionStateManager.AddInteraction(Context, chunkedChannels);

        await _responseHandler.SendChannelsStatsAsync(interaction, Context);
    }
    
    [SlashCommand("canal-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailChannelStats()
    {
//        await Context.Interaction.DeferAsync();

        var channelStats = await _statsService.GetInDetailChannelStats(Context.Channel.Id);
        var interaction = _statInteractionStateManager.AddInteraction(Context, channelStats);

        await _responseHandler.SendDetailedChannelStatsAsync(interaction, Context);
    }

    [SlashCommand("usuario-listar", "efetuar backup deste canal")]
    public async Task ListAllUsersStats()
    {
//        await Context.Interaction.DeferAsync();

        var users = await _statsService.ListAllUsersStats();
        var chunkedUsers = users.Chunk(5).ToArray();
        var interaction = _statInteractionStateManager.AddInteraction(Context, chunkedUsers);

        await _responseHandler.SendUsersStatsAsync(interaction, Context);
    }

    [SlashCommand("usuario-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailUserStats(IUser user)
    {
//        await Context.Interaction.DeferAsync();
        
        var userStats = await _statsService.GetInDetailUserStats(user.Id);
        var interaction = _statInteractionStateManager.AddInteraction(Context, userStats, _responseHandler);
        
        await _responseHandler.SendDetailedUserStatsAsync(interaction, Context);
    }
}