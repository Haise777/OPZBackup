namespace OPZBot.Core.Entities;

public partial class Message
{
    public ulong Id { get; set; }

    public string? Content { get; set; }

    public uint BackupId { get; set; }

    public ulong AuthorId { get; set; }

    public ulong ChannelId { get; set; }

    public DateTime SentDate { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual BackupRegistry Backup { get; set; } = null!;

    public virtual Channel Channel { get; set; } = null!;
}
