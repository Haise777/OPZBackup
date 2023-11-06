using System;
using System.Collections.Generic;

namespace App.Services.Models;

public partial class Backup
{
    public DateTime Date { get; set; }

    public ulong Author { get; set; }

    public ulong YoungestMessage { get; set; }

    public ulong OldestMessage { get; set; }

    public ulong Channel { get; set; }

    public virtual Author AuthorNavigation { get; set; } = null!;

    public virtual Channel ChannelNavigation { get; set; } = null!;
}
