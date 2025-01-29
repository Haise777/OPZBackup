using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;


public class StatsService
{
    private readonly MyDbContext _dbContext;
    private readonly MessageStatsProcessor _messageStatsProcessor;

    public StatsService(MyDbContext dbContext, MessageStatsProcessor messageStatsProcessor)
    {
        _dbContext = dbContext;
        _messageStatsProcessor = messageStatsProcessor;
    }

    // Show a embed with all channels, each containing 
    // N of messages, N of files, bytesize (for future: active period)
    public async Task<IEnumerable<Channel>> ListAllChannelStats()
    {
        return await _dbContext.Channels.ToListAsync();
    }

    public async Task<ChannelStats> GetInDetailChannelStats(ulong channelId)
    {
        var allChannelMessages = await _dbContext.Messages
        .Where(m => m.ChannelId == channelId)
        .ToListAsync();

        var statisticData = new Dictionary<ulong, Metadata>();

        foreach (var message in allChannelMessages)
        {
            if (statisticData.ContainsKey(message.AuthorId))
                statisticData.Add(message.AuthorId, new Metadata());


            statisticData[message.AuthorId].MessageCount++;

            if (message.HasFile)
            {
                var attachments = await _dbContext.AttachmentFiles
                .Where(a => a.MessageId == message.Id)
                .ToListAsync();

                statisticData[message.AuthorId].FileCount += attachments.Count;
                statisticData[message.AuthorId].ByteSize += (ulong)attachments.Sum(a => (long)a.ByteSize);
            }
        }

        var userInChannel = await _dbContext.Users
        .Where(u => statisticData.ContainsKey(u.Id))
        .Select(u => new { u.Id, u.Username })
        .ToListAsync();

        var populatedUsers = new List<User>();

        foreach (var user in userInChannel)
        {
            var statistic = statisticData[user.Id];
            populatedUsers.Add(new User
            {
                Id = user.Id,
                Username = user.Username,
                MessageCount = statistic.MessageCount,
                FileCount = statistic.FileCount,
                ByteSize = statistic.ByteSize,
            });
        }

        var channel = await _dbContext.Channels.FirstAsync(c => c.Id == channelId);

        return new ChannelStats(
            channel.MessageCount,
            channel.FileCount,
            channel.ByteSize,
            populatedUsers
        );
    }

    // Show a embed with all users, each containing
    // N of messages, N of files, bytesize (for future: active period)
    public async Task<IEnumerable<User>> ListAllUsersStats()
    {
        return await _dbContext.Users.ToListAsync();
    }

    // Show a embed with all of the above, plus
    // total num of mention to other users with then number of mention for each individual user
    // each file type sent with their number of sent
    // like: image: 20, video: 7, audio: 2, others: 34
    // top most common words sent inside a message
    // active period
    public async Task GetInDetailUserStats(ulong userId)
    {
        var userMessages = await _dbContext.Messages
        .Where(m => m.AuthorId == userId)
        .ToListAsync();

        var messageStats = await _messageStatsProcessor.AnalyzeMessageListAsync(userMessages);


    }
}