using Discord.Interactions;
using OPZBackup.Data;
using OPZBackup.FileManagement;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services.Backup;

#pragma warning disable CS0618 // Type or member is obsolete
public class BackupContextFactory
{
    private readonly MyDbContext _dbContext;
    private readonly FileCleaner _fileCleaner;
    private readonly Mapper _mapper;
    private readonly StatisticTracker _statisticTracker;

    public BackupContextFactory(MyDbContext dbContext, Mapper mapper, FileCleaner fileCleaner,
        StatisticTracker statisticTracker)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileCleaner = fileCleaner;
        _statisticTracker = statisticTracker;
    }

    public async Task<BackupContext> RegisterNewBackup(SocketInteractionContext interactionContext,
        bool isUntilLastBackup)
    {
        var backupContext = await BackupContext.CreateInstanceAsync
        (
            interactionContext,
            isUntilLastBackup,
            _mapper.Map(interactionContext.Channel),
            _mapper.Map(interactionContext.User),
            _dbContext,
            _fileCleaner,
            _statisticTracker
        );

        return backupContext;
    }
    //BackupContext dependencies
}