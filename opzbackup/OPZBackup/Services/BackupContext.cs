using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services;

public class BackupContext
{
    public BackupRegistry BackupRegistry { get; set; }
    public int MessageCount { get; set; }
    public int FileCount { get; set; }
    public bool IsStopped { get; private set; }

    private readonly MyDbContext _dbContext;
    public readonly bool IsUntilLastBackup;

    private BackupContext(MyDbContext dbContext, bool isUntilLastBackup)
    {
        _dbContext = dbContext;
        IsUntilLastBackup = isUntilLastBackup;
    }

    [Obsolete(message: $"Use {nameof(BackupContextFactory)}.{nameof(BackupContextFactory.RegisterNewBackup)} instead.")]
    public static async Task<BackupContext> CreateInstanceAsync(Channel channel, User author, bool isUntilLastBackup,
        MyDbContext dbContext)
    {
        var backupContext = new BackupContext(dbContext, isUntilLastBackup);
        await backupContext.RegisterNewBackup(channel, author);

        return backupContext;
    }


    public void Stop() => IsStopped = true;

    private async Task RegisterNewBackup(Channel channel, User author)
    {
        var backupRegistry = new BackupRegistry
        {
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };

        if (!await _dbContext.Channels.AnyAsync(c => c.Id == channel.Id))
            _dbContext.Channels.Add(channel);
        if (!await _dbContext.Users.AnyAsync(u => u.Id == author.Id))
            _dbContext.Users.Add(author);

        _dbContext.BackupRegistries.Add(backupRegistry);
        await _dbContext.SaveChangesAsync();
    }
}