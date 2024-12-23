using OPZBackup.Data.Models;
using OPZBackup.FileManagement;

namespace OPZBackup.Services;

public record BackupBatch2(IEnumerable<User> Users, IEnumerable<Message> Messages, IEnumerable<Downloadable> ToDownload);