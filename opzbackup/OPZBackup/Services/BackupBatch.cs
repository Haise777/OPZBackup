using OPZBackup.Data.Models;

namespace OPZBackup.Services;

public record BackupBatch(IEnumerable<User> Users, IEnumerable<Message> Messages, int FileCount);
