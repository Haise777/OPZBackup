namespace OPZBackup.Data.Models;

public class Channel : MetadataBase
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public ulong CompressedByteSize { get; set; }

    public virtual ICollection<BackupRegistry> BackupRegistries { get; set; } = new List<BackupRegistry>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}