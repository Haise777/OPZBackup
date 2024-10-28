using Discord;
using OPZBackup.Data.Models;
using OPZBackup.Services.MessageBackup;

namespace OPZBackup.Services.Utils;

public class BackupUtils
{
    public async Task<BackupBatch> ProcessAsync(IMessage[] fetchedMessages, long registryId, IEnumerable<long> existingMessageIds, IEnumerable<long> existingUserIds)
    {
        var users = new List<User>();
        var messages = new List<Message>();
        var fileCount = 0;
        var concurrentDownloads = new List<Task>();

        try
        {
            foreach (var message in fetchedMessages)
            {
                if (message.Content == "" && message.Author.Id == oldProgram.BotUserId) continue;
                if (blacklistedUsers.Any(u => u == message.Author.Id)) continue;
                if (existingMessageIds.Any(m => m == message.Id))
                {
                    if (IsUntilLastBackup)
                    {
                        EndBackupProcess?.Invoke();
                        break;
                    }

                    continue;
                }

                var mappedMessage = mapper.Map(message, registryId);
                if (message.Attachments.Any())
                {
                    concurrentDownloads.Add(fileBackup.BackupFilesAsync(message));
                    mappedMessage.File =
                        $"Backup/Files/{message.Channel.Id}/{message.Id}{fileBackup.GetExtension(message)}";
                    fileCount += message.Attachments.Count;
                }

                if (!await cache.Users.ExistsAsync(message.Author.Id))
                    users.Add(mapper.Map(message.Author));
                messages.Add(mappedMessage);
            }
        }
        finally
        {
            await Task.WhenAll(concurrentDownloads);
        }

        return new MessageBatchData(users, messages, fileCount);
    }
}