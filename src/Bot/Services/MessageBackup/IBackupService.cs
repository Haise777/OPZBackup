using Discord.Interactions;

namespace OPZBot.Services.MessageBackup;

public interface IBackupService
{
    public Task<TimeSpan> TimeFromLastBackupAsync(SocketInteractionContext context);
    public Task DeleteUserAsync(ulong userId);
}