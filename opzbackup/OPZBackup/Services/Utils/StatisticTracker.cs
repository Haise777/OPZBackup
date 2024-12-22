using System.Collections.Frozen;

namespace OPZBackup.Services.Utils;

public class StatisticTracker
{
    private readonly Statistics _channelStatistics = new();
    private readonly Dictionary<ulong, Statistics> _usersStatistics = new();
    public ulong CompressedFilesSize; //TODO change this later

    public void IncrementMessageCounter(ulong userId)
    {
        if (!_usersStatistics.ContainsKey(userId))
            _usersStatistics.Add(userId, new Statistics());
        
        _usersStatistics[userId].MessageCount++;
        _channelStatistics.MessageCount++;
    }

    public void IncrementFileCounter(ulong userId, int fileCount = 1)
    {
        if (!_usersStatistics.ContainsKey(userId))
            _usersStatistics.Add(userId, new Statistics());
        
        _usersStatistics[userId].FileCount += fileCount;
        _channelStatistics.FileCount += fileCount;
    }

    public void IncrementByteSize(ulong userId, ulong byteSize)
    {
        _usersStatistics[userId].ByteSize += byteSize;
        _channelStatistics.ByteSize += byteSize;
    }

    public FrozenDictionary<ulong, Statistics> GetStatistics()
    {
        return _usersStatistics.ToFrozenDictionary();
    }

    public Statistics GetTotalStatistics()
    {
        return _channelStatistics;
    }
}