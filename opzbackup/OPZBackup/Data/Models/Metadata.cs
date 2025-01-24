namespace OPZBackup.Data.Models;

public class Metadata
{
    public int MessageCount { get; set; }

    public int FileCount { get; set; }

    public ulong ByteSize { get; set; }
}