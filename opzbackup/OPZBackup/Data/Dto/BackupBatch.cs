using Discord;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;

namespace OPZBackup.Services.Backup;

public record BackupBatch(
    int Number,
    IEnumerable<IMessage> RawMessages,
    IEnumerable<Message> ProcessedMessages,
    IEnumerable<Downloadable> Downloadables,
    IEnumerable<User> NewUsers
);