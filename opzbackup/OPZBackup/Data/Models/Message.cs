// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBackup.Data.Models;

public class Message
{
    public ulong Id { get; set; }

    public string? Content { get; set; }

    public uint BackupId { get; set; }

    public ulong AuthorId { get; set; }

    public ulong ChannelId { get; set; }

    public DateTime SentDate { get; set; }

    public string? File { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual BackupRegistry Backup { get; set; } = null!;

    public virtual Channel Channel { get; set; } = null!;
}