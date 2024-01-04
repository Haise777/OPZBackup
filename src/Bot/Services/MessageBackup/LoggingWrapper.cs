// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Microsoft.Extensions.Logging;
using OPZBot.Logging;

#pragma warning disable CS8602
// Dereference of a possibly null reference.

namespace OPZBot.Services.MessageBackup;

public class LoggingWrapper(ILogger<BackupService> logger)
{
    public Task LogStart(object? sender, BackupEventArgs e)
    {
        return logger.LogAsync(LogLevel.Information, null,
            "{service}: Backup {registryId} > Started backup process",
            nameof(BackupService),
            e.Registry.Id);
    }

    public Task LogBatchFinished(object? sender, BackupEventArgs e)
    {
        var backupService = sender as MessageBackupService;

        return logger.LogAsync(LogLevel.Information, null,
            "{service}: Backup {registryId} > Finished batch {bNumber} with {messageCount} saved messages | {fileCount} saved files | {userCount} new users",
            nameof(BackupService),
            e.Registry.Id,
            backupService.BatchNumber,
            e.MessageBatch.Messages.Count(),
            e.MessageBatch.FileCount,
            e.MessageBatch.Users.Count());
    }

    public Task LogCompleted(object? sender, BackupEventArgs e)
    {
        var backupService = sender as MessageBackupService;

        return logger.LogAsync(LogLevel.Information, null,
            "{service}: Backup {registryId} > completed at batch number {batchNumber} with {savedNumber} saved messages and {fileNumbers} saved files",
            nameof(BackupService),
            e.Registry.Id,
            backupService.BatchNumber,
            backupService.SavedMessagesCount,
            backupService.SavedFilesCount);
    }

    public Task LogEmptyMessageBackupAttempt(object? sender, BackupEventArgs args)
    {
        return logger.LogAsync(LogLevel.Information, null,
            "{service}: Invalid backup attempt > There was no valid message to backup", nameof(BackupService));
    }

    public Task LogBackupCancelled(object? sender, BackupEventArgs e)
    {
        return logger.LogAsync(LogLevel.Information, null,
            "{service}: Backup {registryId} was cancelled", nameof(BackupService), e.Registry.Id);
    }
}