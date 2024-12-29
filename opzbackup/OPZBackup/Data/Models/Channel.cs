namespace OPZBackup.Data.Models;

public class Channel
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    //TODO move these metadata fields to a base class

    public int MessageCount { get; set; }

    public int FileCount { get; set; }

    public ulong ByteSize { get; set; }

    public ulong CompressedByteSize { get; set; }

    public virtual ICollection<BackupRegistry> BackupRegistries { get; set; } = new List<BackupRegistry>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}