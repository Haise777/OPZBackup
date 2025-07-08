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
    private readonly BackupPerformanceProfiler _backupPerformanceProfiler;

    public BackupContextFactory(FileCleaner fileCleaner,
        StatisticTracker statisticTracker,
        BackupLoggerFactory backupLoggerFactory,
        BackupPerformanceProfiler backupPerformanceProfiler)
    {
        _fileCleaner = fileCleaner;
        _statisticTracker = statisticTracker;
        _backupLoggerFactory = backupLoggerFactory;
        _backupPerformanceProfiler = backupPerformanceProfiler;
    }

    public BackupContext Create(bool isUntilLastBackup, BackupRegistry backupRegistry)
    {
        var backupContext = new BackupContext(isUntilLastBackup, _fileCleaner, _statisticTracker, backupRegistry,_backupPerformanceProfiler, _backupLoggerFactory);

        return backupContext;
    }
    //BackupContext dependencies
}