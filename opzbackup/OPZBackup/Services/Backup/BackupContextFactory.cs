using Discord.Interactions;
using OPZBackup.Data;
using OPZBackup.FileManagement;

namespace OPZBackup.Services.Backup;

#pragma warning disable CS0618 // Type or member is obsolete
public class BackupContextFactory
{
    //BackupContext dependencies
    private readonly MyDbContext _dbContext;
    private readonly Mapper _mapper;
    private readonly FileCleaner _fileCleaner;

    public BackupContextFactory(MyDbContext dbContext, Mapper mapper, FileCleaner fileCleaner)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileCleaner = fileCleaner;
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
            _fileCleaner
        );

        return backupContext;
    }
}