// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBot.DataAccess.Caching;

public class IdCacheManager
{
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

    public IDataCache<ulong> UserIds { get; }
    public IDataCache<ulong> ChannelIds { get; }
    public IDataCache<uint> BackupRegistryIds { get; } //TODO Only worth in a multiple parallel backup scenario
}