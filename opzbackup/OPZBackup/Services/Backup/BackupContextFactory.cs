using Discord.Interactions;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services.Backup;

public class BackupContextFactory
{
    private readonly FileCleaner _fileCleaner;
    private readonly StatisticTracker _statisticTracker;
    private readonly BackupLoggerFactory _backupLoggerFactory;

    public BackupContextFactory(FileCleaner fileCleaner,
        StatisticTracker statisticTracker,
        BackupLoggerFactory backupLoggerFactory)
    {
        _fileCleaner = fileCleaner;
        _statisticTracker = statisticTracker;
        _backupLoggerFactory = backupLoggerFactory;
    }

    public BackupContext Create(bool isUntilLastBackup, BackupRegistry backupRegistry)
    {
        var backupContext = new BackupContext(isUntilLastBackup, _fileCleaner, _statisticTracker, backupRegistry, _backupLoggerFactory);

        return backupContext;
    }
    //BackupContext dependencies
}