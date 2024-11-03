// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBot.Data.Context;
using OPZBot.Data.Models;
using OPZBot.Data;
using OPZBot.Data.Caching;
using OPZBot.Services.MessageBackup.FileBackup;

namespace OPZBot.Services.MessageBackup;

public class MessageProcessor(
    MyDbContext dataContext,
    IdCacheManager cache,
    Mapper mapper,
    IFileBackupService fileBackup)
    : IBackupMessageProcessor
{
    public event Action? EndBackupProcess;
    public bool IsUntilLastBackup { get; set; }

    public async Task<MessageBatchData> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint registryId,
        CancellationToken cToken)
    {
        var existingMessageIds = await dataContext.Messages
            .Where(x => x.ChannelId == messageBatch.First().Channel.Id)
            .Select(m => m.Id)
            .ToArrayAsync();
        var blacklistedUsers = await dataContext.Users
            .Where(u => u.IsBlackListed == true)
            .Select(u => u.Id)
            .ToArrayAsync();

        var users = new List<User>();
        var messages = new List<Message>();
        var fileCount = 0;
        var concurrentDownloads = new List<Task>();

        try
        {
            foreach (var message in messageBatch)
            {
                cToken.ThrowIfCancellationRequested();
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