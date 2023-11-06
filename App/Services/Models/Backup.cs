using System;
using System.Collections.Generic;

namespace App.Services.Models;

public partial class Backup
{
    public DateTime Date { get; set; }

    public ulong Author { get; set; }

    public ulong YoungestMessage { get; set; }

    public ulong OldestMessage { get; set; }

    public virtual User AuthorNavigation { get; set; } = null!;
}
