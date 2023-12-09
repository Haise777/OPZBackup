using System;
using System.Collections.Generic;

namespace Data.Contracts.Entities;

public partial class Channel
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<BackupRegistry> BackupRegistries { get; set; } = new List<BackupRegistry>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
