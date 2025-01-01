using Discord.Interactions;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services.Backup;

public class BackupContextFactory
{
    private readonly FileCleaner _fileCleaner;
    private readonly StatisticTracker _statisticTracker;

    public BackupContextFactory(FileCleaner fileCleaner,
        StatisticTracker statisticTracker)
    {
        _fileCleaner = fileCleaner;
        _statisticTracker = statisticTracker;
    }

    public BackupContext Create(SocketInteractionContext interactionContext,
        bool isUntilLastBackup, BackupRegistry backupRegistry)
    {
        var backupContext = new BackupContext(isUntilLastBackup, _fileCleaner, _statisticTracker, backupRegistry);

        return backupContext;
    }
    //BackupContext dependencies
}