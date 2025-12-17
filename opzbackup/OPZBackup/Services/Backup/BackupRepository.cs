using OPZBackup.Data;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.Backup;

public class BackupRepository : IBackupRepository
{
    private readonly MyDbContext _dbContext;

    public BackupRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void SaveMessages(IEnumerable<Message> messages)
    {
        _dbContext.Messages.AddRange(messages);
    }

    public void SaveUsers(IEnumerable<User> users)
    {
        _dbContext.Users.AddRange(users);
    }

    public void SaveAttachments(IEnumerable<AttachmentFile> attachments)
    {
        _dbContext.AttachmentFiles.AddRange(attachments);
    }

    public async Task CommitChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}