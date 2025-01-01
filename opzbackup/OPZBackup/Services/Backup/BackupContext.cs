using Discord;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services.Backup;

public class BackupContext
{
    private readonly FileCleaner _fileCleaner;

    public BackupContext(bool isUntilLastBackup,
        FileCleaner fileCleaner, StatisticTracker statisticTracker,
        BackupRegistry backupRegistry)
    {
        IsUntilLastBackup = isUntilLastBackup;
        _fileCleaner = fileCleaner;
        StatisticTracker = statisticTracker;
        BackupRegistry = backupRegistry;
    }

    public BackupRegistry BackupRegistry { get; private set; }
    public StatisticTracker StatisticTracker { get; private set; }
    public bool IsStopped { get; private set; }
    public int MessageCount => StatisticTracker.GetTotalStatistics().MessageCount;
    public int FileCount => StatisticTracker.GetTotalStatistics().FileCount;
    public bool IsUntilLastBackup { get; }
    public int BatchNumber { get; set; }
    public TimeSpan AverageBatchTime { get; set; }
    // public Message? StartMessage { get; set; }
    // public Message? LastMessage { get; set; }

    public async Task RollbackAsync()
    {
        IsStopped = true;
        await _fileCleaner.DeleteDirAsync(App.TempPath);
    }

    public void Stop()
    {
        IsStopped = true;
    }
}