using System;
using System.Collections.Generic;

namespace App.Services.Models;

public partial class Channel
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
