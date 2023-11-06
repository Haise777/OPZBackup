using System;
using System.Collections.Generic;

namespace App.Services.Models;

public partial class Author
{
    public ulong Id { get; set; }

    public string Username { get; set; } = null!;

    public virtual ICollection<Backup> Backups { get; set; } = new List<Backup>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
