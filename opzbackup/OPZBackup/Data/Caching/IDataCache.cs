// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBackup.Data.Caching;

public interface IDataCache<T>
{
    Task<IDataCache<T>> AddAsync(T item);
    Task<IDataCache<T>> AddRangeAsync(IEnumerable<T> items);
    Task<IDataCache<T>> UpdateRangeAsync(IEnumerable<T> items);
    Task<bool> ExistsAsync(T item, bool shouldCache = true);
    Task RemoveAsync(T item);
}