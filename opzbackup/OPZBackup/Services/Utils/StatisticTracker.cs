using System.Collections.Frozen;
using System.Collections.Immutable;

namespace OPZBackup.Services.Utils;

public class StatisticTracker
{
    private readonly Dictionary<ulong, Statistics> _usersStatistics = new();
    public ulong CompressedFilesSize; //TODO change this later

    public void IncrementMessageCounter(ulong userId, int messageCount = 1)
    {
        AddEntryIfNotExists(userId);

        _usersStatistics[userId].MessageCount += messageCount;
    }

    public void IncrementFileCounter(ulong userId, int fileCount = 1)
    {
        AddEntryIfNotExists(userId);

        _usersStatistics[userId].FileCount += fileCount;
    }

    public void IncrementByteSize(ulong userId, ulong byteSize)
    {
        AddEntryIfNotExists(userId);
        
        _usersStatistics[userId].ByteSize += byteSize;
    }
    
    public ImmutableDictionary<ulong, Statistics> GetStatistics()
    {
        return _usersStatistics.ToImmutableDictionary();
    }

    public Statistics GetTotalStatistics()
    {
        var totalStatistics = new Statistics();

        foreach (var entry in _usersStatistics.Keys)
        {
            totalStatistics.MessageCount += _usersStatistics[entry].MessageCount;
            totalStatistics.FileCount += _usersStatistics[entry].FileCount;
            totalStatistics.ByteSize += _usersStatistics[entry].ByteSize;
        }

        return totalStatistics;
    }
    
    private void AddEntryIfNotExists(ulong userId)
    {
        if (!_usersStatistics.ContainsKey(userId))
            _usersStatistics.Add(userId, new Statistics());
    }
}