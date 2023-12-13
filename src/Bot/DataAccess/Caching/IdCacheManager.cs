namespace OPZBot.DataAccess.Caching;

public class IdCacheManager
{
    public IDataCache<ulong> UserIds { get;}
    public IDataCache<ulong> ChannelIds { get;}
    
    public IdCacheManager(IDataCache<ulong> userIds, IDataCache<ulong> channelIds)
    {
        UserIds = userIds;
        ChannelIds = channelIds;
    }
    
    public IdCacheManager()
    {
        UserIds = new DataCache<ulong>();
        ChannelIds = new DataCache<ulong>();
    }
}