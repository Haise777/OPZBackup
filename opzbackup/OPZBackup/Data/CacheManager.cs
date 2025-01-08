using System.Collections.Concurrent;

namespace OPZBackup.Data;


public class CacheManager 
{
    private ConcurrentDictionary<ulong, bool> _cachedUserIds;
    private ConcurrentDictionary<ulong, bool> _cachedMessageIds;

    public CacheManager(IEnumerable<ulong> userIds, IEnumerable<ulong> messageIds)
    {
        var userIdsDictionary = userIds.ToDictionary(x => x, x => true);
        var messageIdsDictionary = messageIds.ToDictionary(x => x, x => true);

        _cachedUserIds = new ConcurrentDictionary<ulong, bool>(userIdsDictionary);
        _cachedMessageIds = new ConcurrentDictionary<ulong, bool>(messageIdsDictionary);
    }

    public bool IsUserIdCached(ulong userId)
    {

        if (_cachedUserIds.ContainsKey(userId))
            return true;
        

        _cachedUserIds.TryAdd(userId, true);
        return false;
    }

    public bool IsMessageIdCached(ulong messageId)
    {

        if (_cachedMessageIds.ContainsKey(messageId))
            return true;
        

        _cachedMessageIds.TryAdd(messageId, true);
        return false;
    }

}