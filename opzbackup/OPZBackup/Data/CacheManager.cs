using System.Collections.Concurrent;

namespace OPZBackup.Data;


public class CacheManager 
{
    private ConcurrentDictionary<ulong, bool> _cachedUserIds;
    private ConcurrentDictionary<ulong, bool> _cachedMessageIds;

    public CacheManager()
    {
        _cachedUserIds = new ConcurrentDictionary<ulong, bool>();
        _cachedMessageIds = new ConcurrentDictionary<ulong, bool>();
    }

    public CacheManager(IEnumerable<ulong> userIds, IEnumerable<ulong> messageIds)
    {
        var userIdsDictionary = userIds.ToDictionary(x => x, _ => true);
        var messageIdsDictionary = messageIds.ToDictionary(x => x, _ => true);

        _cachedUserIds = new ConcurrentDictionary<ulong, bool>(userIdsDictionary);
        _cachedMessageIds = new ConcurrentDictionary<ulong, bool>(messageIdsDictionary);
    }

    public bool IsUserIdCached(ulong userId)
    {
        if (_cachedUserIds.ContainsKey(userId))
            return true;

        var result = _cachedUserIds.TryAdd(userId, true);
        return !result;
    }

    public bool IsMessageIdCached(ulong messageId)
    {

        if (_cachedMessageIds.ContainsKey(messageId))
            return true;
        
        var result = _cachedMessageIds.TryAdd(messageId, true);
        return !result;
    }

    public void RepopulateCache(List<ulong> userIds, List<ulong> messageIds)
    {
        var userIdsDictionary = userIds.ToDictionary(x => x, _ => true);
        var messageIdsDictionary = messageIds.ToDictionary(x => x, _ => true);

        _cachedUserIds = new ConcurrentDictionary<ulong, bool>(userIdsDictionary);
        _cachedMessageIds = new ConcurrentDictionary<ulong, bool>(messageIdsDictionary);
    }
}