// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using OPZBot.Utilities;

namespace OPZBot.Services.MessageBackup;

public interface IBackupMessageService : IBackupService
{
    int BatchNumber { get; }
    int SavedMessagesCount { get; }
    int SavedFilesCount { get; }
    event AsyncEventHandler<BackupEventArgs>? StartedBackupProcess;
    event AsyncEventHandler<BackupEventArgs>? FinishedBatch;
    event AsyncEventHandler<BackupEventArgs>? CompletedBackupProcess;
    event AsyncEventHandler<BackupEventArgs>? ProcessHasFailed;
    event AsyncEventHandler<BackupEventArgs>? EmptyBackupAttempt;
    Task StartBackupAsync(SocketInteractionContext context, bool isUntilLastBackup);
}