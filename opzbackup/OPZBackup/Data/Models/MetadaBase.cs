namespace OPZBackup.Data.Models;

public abstract class MetadataBase
{
    public int MessageCount { get; set; }

    public int FileCount { get; set; }

    public ulong ByteSize { get; set; }
}