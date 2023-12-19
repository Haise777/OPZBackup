using Discord;

namespace OPZBot.Services.MessageBackup.FileBackup;

public interface IFileBackupService
{
    Task BackupFilesAsync(IMessage message);
}