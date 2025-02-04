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

    public async Task<IEnumerable<Channel>> ListAllChannelStats()
    {
        return await _dbContext.Channels.ToListAsync();
    }

    private record UserData(ulong Id, string Username);

    public async Task<ChannelStats> GetInDetailChannelStats(ulong channelId)
    {
        var allChannelMessages = await _dbContext.Messages
            .Where(m => m.ChannelId == channelId)
            .ToListAsync();

        var statisticData = new Dictionary<ulong, Metadata>();

        foreach (var message in allChannelMessages)
        {
            if (!statisticData.ContainsKey(message.AuthorId))
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
        
        var allUserIds = statisticData.Keys.ToList();

        var userInChannel = await _dbContext.Users
            .Where(u => allUserIds.Contains(u.Id))
            .Select(u => new UserData(u.Id, u.Username))
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

    public async Task<IEnumerable<User>> ListAllUsersStats()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<UserStats> GetInDetailUserStats(ulong userId)
    {
        var userMessages = await _dbContext.Messages
            .Where(m => m.AuthorId == userId)
            .Include(m => m.Attachments)
            .ToListAsync();

        var user = await _dbContext.Users.FirstAsync(u => u.Id == userId);
        var userStats = await _messageStatsProcessor.AnalyzeMessageListAsync(userMessages);

        return new UserStats(
            user,
            userStats.NumberOfMentions,
            userStats.MostCommonWords,
            userStats.fileTypeStats
            );
    }
}