using System;
using System.Collections.Generic;

namespace OPZBot.DataAccess.Models;

public partial class User
{
    public ulong Id { get; set; }

    public string Username { get; set; } = null!;

    public virtual ICollection<BackupRegistry> BackupRegistries { get; set; } = new List<BackupRegistry>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
