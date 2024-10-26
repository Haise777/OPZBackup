// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using OPZBackup.Utilities;

namespace OPZBackup.Services.MessageBackup;

public interface IMessageBackupService : IBackupService
{
    CancellationTokenSource CancelSource { get; }
    event AsyncEventHandler<BackupEventArgs>? StartedBackupProcess;
    event AsyncEventHandler<BackupEventArgs>? FinishedBatch;
    event AsyncEventHandler<BackupEventArgs>? CompletedBackupProcess;
    event AsyncEventHandler<BackupEventArgs>? ProcessFailed;
    event AsyncEventHandler<BackupEventArgs>? EmptyBackupAttempt;
    event AsyncEventHandler<BackupEventArgs>? ProcessCanceled;
    Task StartBackupAsync(SocketInteractionContext context, bool isUntilLastBackup);
}