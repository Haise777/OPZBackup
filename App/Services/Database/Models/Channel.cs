using System;
using System.Collections.Generic;

namespace App.Services.Database.Models;

public partial class Channel
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<BackupRegister> BackupRegisters { get; set; } = new List<BackupRegister>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
