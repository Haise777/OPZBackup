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

    //Command structure should be like
    // Stats -> Channel/User -> ListAll
    // Stats -> Channel -> InDetails
    // Stats -> User -> InDetails -> @<userMention>

    // Show a embed with all channels, each containing 
    // N of messages, N of files, bytesize (for future: active period)
    [SlashCommand("canal-listar", "efetuar backup deste canal")]
    public async Task ListAllChannelStats()
    {
        await Context.Interaction.DeferAsync();

        var channels = await _statsService.ListAllChannelStats();
        await _responseHandler.SendChannelsStatsAsync(channels, Context);
    }

    // Show a embed with all of the above, plus
    // Each user that sent message at this channel and their
    // N of messages sent to this channel
    // N of files sent to this channel
    [SlashCommand("canal-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailChannelStats()
    {
        await Context.Interaction.DeferAsync();

        var channelStats = await _statsService.GetInDetailChannelStats(Context.Channel.Id);
        await _responseHandler.SendDetailedChannelStatsAsync(channelStats, Context);
    }

    // Show a embed with all users, each containing
    // N of messages, N of files, bytesize (for future: active period)
    [SlashCommand("usuario-listar", "efetuar backup deste canal")]
    public async Task ListAllUsersStats()
    {
        await Context.Interaction.DeferAsync();

        var users = await _statsService.ListAllUsersStats();
        await _responseHandler.SendUsersStatsAsync(users, Context);
    }

    // Show a embed with all of the above, plus
    // total num of mention to other users with then number of mention for each individual user
    // each file type sent with their number of sent
    // like: image: 20, video: 7, audio: 2, others: 34
    // top most common words sent inside a message
    // active period
    [SlashCommand("usuario-detalhado", "efetuar backup deste canal")]
    public async Task GetInDetailUserStats()
    {
        await Context.Interaction.DeferAsync();

        
        var userStats = await _statsService.GetInDetailUserStats(Context.User.Id);
        await _responseHandler.SendDetailedUserStatsAsync(userStats, Context);
    }
}