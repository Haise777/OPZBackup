
using OPZBackup.Data;
using OPZBackup.Data.Models;

namespace OPZBackup.Services;

#pragma warning disable CS0618 // Type or member is obsolete
public class BackupContextFactory
{
    //BackupContext dependencies
    private readonly MyDbContext _dbContext;

    public BackupContextFactory(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BackupContext> RegisterNewBackup(Channel channel, User author, bool isUntilLastBackup)
    {
        var backupContext = await BackupContext.CreateInstanceAsync
        (
            channel, author, isUntilLastBackup,
            _dbContext
        );

        return backupContext;
    }
}