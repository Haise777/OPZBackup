namespace OPZBackup.Services.Utils;

public class Statistics
{

    public Statistics(int messageCount = 0, int fileCount = 0, ulong byteSize = 0)
    {
        MessageCount = messageCount;
        FileCount = fileCount;
        ByteSize = byteSize;
    }

    public int MessageCount { get; set; }
    public int FileCount { get; set; }

    public ulong ByteSize { get; set; }
}