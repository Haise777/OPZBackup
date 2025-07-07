using OPZBackup.Data.Models;

namespace OPZBackup.Services.Backup;


public interface IBackupRepository
{
    Task CommitChangesAsync();
    void SaveAttachments(IEnumerable<AttachmentFile> attachments);
    void SaveMessages(IEnumerable<Message> messages);
    void SaveUsers(IEnumerable<User> users);
}