// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;

namespace OPZBackup.Services.MessageBackup;

public interface IBackupResponseHandler : IResponseHandler
{
    Task SendStartNotificationAsync(object? sender, BackupEventArgs e);
    Task SendBatchFinishedAsync(object? sender, BackupEventArgs e);
    Task SendCompletedAsync(object? sender, BackupEventArgs e);
    Task SendFailedAsync(object? sender, BackupEventArgs e);
    Task SendProcessCancelledAsync(object? sender, BackupEventArgs e);
    Task SendInvalidAttemptAsync(SocketInteractionContext context, TimeSpan cooldownTime);
    Task SendDeleteConfirmationAsync(SocketInteractionContext context);
    Task SendUserDeletionResultAsync(SocketInteractionContext context, bool wasDeleted);
    Task SendEmptyMessageBackupAsync(object? sender, BackupEventArgs args);
    Task SendAlreadyInProgressAsync(SocketInteractionContext context);
    Task SendProcessToCancelAsync(SocketInteractionContext context, bool noCurrentBackup = false);
}