using Discord.Interactions;

namespace OPZBackup.Modules;

public class StatsModule : InteractionModuleBase<SocketInteractionContext>
{

    public async Task ListAllChannelStats()
    {
        // Show a embed with all channels, each containing 
        // N of messages, N of files, bytesize (for future: active period)
    }

    public async Task GetInDetailChannelStats()
    {
        // Show a embed with all of the above, plus
        // Each user that sent message at this channel and their
        // N of messages sent to this channel
        // N of files sent to this channel
    }

    public async Task ListAllUsersStats()
    {
        // Show a embed with all users, each containing
        // N of messages, N of files, bytesize (for future: active period)
    }

    public async Task GetInDetailUserStats()
    {
        // Show a embed with all of the above, plus
        // total num of mention to other users with then number of mention for each individual user
        // each file type sent with their number of sent
        // like: image: 20, video: 7, audio: 2, others: 34
        // top most common words sent inside a message
        // active period
    }

}