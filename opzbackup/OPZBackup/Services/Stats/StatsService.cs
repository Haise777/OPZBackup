using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;

namespace OPZBackup.Services.Stats;


public class StatsService
{
    private readonly MyDbContext _dbContext;

    public StatsService(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }


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

    // Show a embed with all users, each containing
    // N of messages, N of files, bytesize (for future: active period)
    public async Task ListAllUsersStats()
    {
        var allUsers = await _dbContext.Users.ToListAsync();

        foreach (var user in allUsers)
        {
            //Get individual user information here
        }

    }

    // Show a embed with all of the above, plus
    // total num of mention to other users with then number of mention for each individual user
    // each file type sent with their number of sent
    // like: image: 20, video: 7, audio: 2, others: 34
    // top most common words sent inside a message
    // active period
    public async Task GetInDetailUserStats()
    {
        ulong stubUserId = 242142;

        var userMessages = await _dbContext.Messages.Where(m => m.AuthorId == stubUserId).ToListAsync();



    }
}