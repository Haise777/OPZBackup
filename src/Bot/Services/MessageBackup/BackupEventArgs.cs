// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class BackupEventArgs : EventArgs
{
    public BackupEventArgs(SocketInteractionContext? context = null, BackupRegistry? registry = null,
        MessageDataBatchDto? messageBatch = null)
    {
        InteractionContext = context;
        Registry = registry;
        MessageBatch = messageBatch;
    }

    public SocketInteractionContext? InteractionContext { get; set; }
    public BackupRegistry? Registry { get; set; }
    public MessageDataBatchDto? MessageBatch { get; set; }
}