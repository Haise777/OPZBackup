// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBackup.DataAccess.Models;

public class Channel
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<BackupRegistry> BackupRegistries { get; set; } = new List<BackupRegistry>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}