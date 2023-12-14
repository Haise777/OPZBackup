namespace OPZBot.DataAccess.Caching;

public class IdCacheManager
{
    public IDataCache<ulong> UserIds { get;}
    public IDataCache<ulong> ChannelIds { get;}
    public IDataCache<uint> BackupRegistryIds { get;} //TODO Only worth in a multiple parallel backup scenario
    
    public IdCacheManager(IDataCache<ulong> userIds, IDataCache<ulong> channelIds, IDataCache<uint> backupRegistryIds)
    {
        UserIds = userIds;
        ChannelIds = channelIds;
        BackupRegistryIds = backupRegistryIds;
    }
    
    public IdCacheManager()
    {
        BackupRegistryIds = new DataCache<uint>();
        UserIds = new DataCache<ulong>();
        ChannelIds = new DataCache<ulong>();
    }
    
    
}