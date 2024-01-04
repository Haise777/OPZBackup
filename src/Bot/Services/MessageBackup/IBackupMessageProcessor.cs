// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;

namespace OPZBot.Services.MessageBackup;

public interface IBackupMessageProcessor
{
    bool IsUntilLastBackup { get; set; }
    event Action? EndBackupProcess;

    Task<MessageBatchData> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint backupId,
        CancellationToken cToken);
}