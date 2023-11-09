using System;
using System.Collections.Generic;

namespace App.Services.Models;

public partial class BackupRegister
{
    public DateTime Date { get; set; }

    public ulong? AuthorId { get; set; }

    public ulong? StartMessageId { get; set; }

    public ulong? EndMessageId { get; set; }

    public ulong ChannelId { get; set; }

    public virtual Author? Author { get; set; }

    public virtual Channel Channel { get; set; } = null!;

    public virtual Message? EndMessage { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Message? StartMessage { get; set; }
}
