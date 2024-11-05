using Discord.Interactions;
using OPZBackup.Data;
using OPZBackup.Data.Models;

namespace OPZBackup.Services;

#pragma warning disable CS0618 // Type or member is obsolete
public class BackupContextFactory
{
    //BackupContext dependencies
    private readonly MyDbContext _dbContext;
    private readonly Mapper _mapper;

    public BackupContextFactory(MyDbContext dbContext, Mapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
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
            _dbContext
        );

        return backupContext;
    }
}