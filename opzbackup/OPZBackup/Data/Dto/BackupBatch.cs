using Discord;
using Discord.WebSocket;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services.Backup;


public record BackupBatch(
    int Number,
    IEnumerable<IMessage> RawMessages,
    IEnumerable<Message> ProcessedMessages,
    IEnumerable<Downloadable> Downloadables,
    IEnumerable<User> NewUsers
);