// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel.DataAnnotations;

namespace OPZBackup.Data.Models;

public class BackupRegistry
{
    [Key]
    public uint Id { get; set; }

    public ulong? AuthorId { get; set; }

    public ulong ChannelId { get; set; }

    public DateTime Date { get; set; }

    public virtual User? Author { get; set; }

    public virtual Channel Channel { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}