// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;

namespace OPZBot.Services.MessageBackup;

public interface IResponseHandler
{
    Task SendStartNotificationAsync(object? sender, BackupEventArgs e);
    Task SendBatchFinishedAsync(object? sender, BackupEventArgs e);
    Task SendCompletedAsync(object? sender, BackupEventArgs e);
    Task SendFailedAsync(object? sender, BackupEventArgs e);
    Task SendInvalidAttemptAsync(SocketInteractionContext context, TimeSpan cooldownTime);
    Task SendDeleteConfirmationAsync(SocketInteractionContext context);
    Task SendUserDeletionResultAsync(SocketInteractionContext context, bool wasDeleted);
    Task SendEmptyBackupAsync(object? sender, BackupEventArgs args);
}