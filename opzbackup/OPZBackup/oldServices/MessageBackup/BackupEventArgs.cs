// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.MessageBackup;

public class BackupEventArgs(
    SocketInteractionContext? context = null,
    BackupRegistry? registry = null,
    MessageBatchData? messageBatch = null)
    : EventArgs
{
    public SocketInteractionContext? InteractionContext { get; set; } = context;
    public BackupRegistry? Registry { get; set; } = registry;
    public MessageBatchData? MessageBatch { get; set; } = messageBatch;
}