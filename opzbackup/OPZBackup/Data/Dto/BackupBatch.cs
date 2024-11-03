using OPZBackup.Data.Models;
using OPZBackup.FileManagement;

namespace OPZBackup.Services;

public record BackupBatch(IEnumerable<User> Users, IEnumerable<Message> Messages, IEnumerable<Downloadable> ToDownload);
