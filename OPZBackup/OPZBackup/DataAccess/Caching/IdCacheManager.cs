// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBackup.DataAccess.Caching;

public class IdCacheManager
{
    public IdCacheManager(IDataCache<ulong> users, IDataCache<ulong> channelIds, IDataCache<uint> backupRegistryIds)
    {
        Users = users;
        ChannelIds = channelIds;
    }

    public IdCacheManager()
    {
        Users = new DataCache<ulong>();
        ChannelIds = new DataCache<ulong>();
    }

    public IDataCache<ulong> Users { get; }
    public IDataCache<ulong> ChannelIds { get; }
}