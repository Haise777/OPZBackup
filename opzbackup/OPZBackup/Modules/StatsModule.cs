using Discord.Interactions;
using OPZBackup.ResponseHandlers.Stats;
using OPZBackup.Services.Stats;

namespace OPZBackup.Modules;

[Group("stats", "utilizar a função de stats")]
public class StatsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly StatsService _statsService;
    private readonly ResponseHandler _responseHandler;
    public StatsModule(StatsService statsService, ResponseHandler responseHandler)
    {
        _statsService = statsService;
        _responseHandler = responseHandler;
    }
    
    [SlashCommand("canal-listar", "efetuar backup deste canal")]
    public async Task ListAllChannelStats()
    {
        await Context.Interaction.DeferAsync();

        var channels = await _statsService.ListAllChannelStats();
        throw new NotImplementedException("Cabum");
        await _responseHandler.SendChannelsStatsAsync(channels, Context);
    }
    
    [SlashCommand("canal-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailChannelStats()
    {
        await Context.Interaction.DeferAsync();

        var channelStats = await _statsService.GetInDetailChannelStats(Context.Channel.Id);
        await _responseHandler.SendDetailedChannelStatsAsync(channelStats, Context);
    }
    
    [SlashCommand("usuario-listar", "efetuar backup deste canal")]
    public async Task ListAllUsersStats()
    {
        await Context.Interaction.DeferAsync();

        var users = await _statsService.ListAllUsersStats();
        await _responseHandler.SendUsersStatsAsync(users, Context);
    }
    
    [SlashCommand("usuario-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailUserStats()
    {
        await Context.Interaction.DeferAsync();

        
        var userStats = await _statsService.GetInDetailUserStats(Context.User.Id);
        await _responseHandler.SendDetailedUserStatsAsync(userStats, Context);
    }
}