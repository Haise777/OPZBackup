namespace App.Services.Models;

public partial class Message
{
    public ulong Id { get; set; }

    public string Author { get; set; } = null!;

    public DateTime Date { get; set; }

    public DateTime? EditDate { get; set; }

    public string Content { get; set; } = null!;

    public ulong ChannelId { get; set; }

    public virtual Channel Channel { get; set; } = null!;
}
