using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services;

#pragma warning disable CS0618 // Type or member is obsolete
public class BackupContextFactory
{
    //BackupContext dependencies
    private readonly MyDbContext _dbContext;
    private readonly AttachmentDownloader _attachmentDownloader;

    public BackupContextFactory(MyDbContext dbContext, AttachmentDownloader attachmentDownloader)
    {
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
    }

    public async Task<BackupContext> RegisterNewBackup(Channel channel, User author, bool isUntilLastBackup)
    {
        var backupContext = await BackupContext.CreateInstanceAsync
        (
            channel, author, isUntilLastBackup,
            _attachmentDownloader, 
            _dbContext
        );

        return backupContext;
    }
}