using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;

namespace OPZBackup.Services.Backup;

public class BackupContext
{
    private readonly MyDbContext _dbContext;
    private readonly FileCleaner _fileCleaner;

    private BackupContext(SocketInteractionContext interactionContext, MyDbContext dbContext, bool isUntilLastBackup,
        FileCleaner fileCleaner)
    {
        InteractionContext = interactionContext;
        _dbContext = dbContext;
        IsUntilLastBackup = isUntilLastBackup;
        _fileCleaner = fileCleaner;
    }

    public BackupRegistry BackupRegistry { get; private set; } = null!;
    public SocketInteractionContext InteractionContext { get; private set; }
    public bool IsStopped { get; private set; }
    public int MessageCount { get; set; }
    public int FileCount { get; set; }
    public bool IsUntilLastBackup { get; }
    public int BatchNumber { get; set; }

    public async Task RollbackAsync()
    {
        IsStopped = true;
        _dbContext.BackupRegistries.Remove(BackupRegistry);
        await _dbContext.SaveChangesAsync();

        await _fileCleaner.DeleteDirAsync(App.TempFilePath);
    }

    public void Stop()
    {
        IsStopped = true;
    }

    [Obsolete($"Use {nameof(BackupContextFactory)}.{nameof(BackupContextFactory.RegisterNewBackup)} instead.")]
    public static async Task<BackupContext> CreateInstanceAsync(SocketInteractionContext interactionContext,
        bool isUntilLastBackup,
        Channel channel,
        User author,
        MyDbContext dbContext,
        FileCleaner fileCleaner)
    {
        var backupContext = new BackupContext(interactionContext, dbContext, isUntilLastBackup, fileCleaner);
        await backupContext.RegisterNewBackup(channel, author);

        return backupContext;
    }

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

        BackupRegistry = backupRegistry;
    }
}