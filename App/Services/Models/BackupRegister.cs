using System;
using System.Collections.Generic;

namespace App.Services.Models;

public partial class BackupRegister
{
    public DateTime Date { get; set; }

    public ulong Author { get; set; }

    public ulong? YoungestMessage { get; set; }

    public ulong? OldestMessage { get; set; }

    public ulong ChannelId { get; set; }

    public virtual Author AuthorNavigation { get; set; } = null!;

    public virtual Channel Channel { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Message? OldestMessageNavigation { get; set; }

    public virtual Message? YoungestMessageNavigation { get; set; }
}
