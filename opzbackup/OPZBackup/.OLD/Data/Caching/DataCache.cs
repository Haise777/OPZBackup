// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBot.Data.Caching;

public class DataCache<T> : IDataCache<T>, IDisposable
{
    private readonly List<T> _cachedData = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IDataCache<T>> AddAsync(T item)
    {
        await _lock.WaitAsync();
        try
        {
            _cachedData.Add(item);
        }
        finally
        {
            _lock.Release();
        }

        return this;
    }

    public async Task<IDataCache<T>> AddRangeAsync(IEnumerable<T> items)
    {
        await _lock.WaitAsync();
        try
        {
            _cachedData.AddRange(items);
        }
        finally
        {
            _lock.Release();
        }

        return this;
    }

    public async Task<IDataCache<T>> UpdateRangeAsync(IEnumerable<T> items)
    {
        await _lock.WaitAsync();
        try
        {
            _cachedData.Clear();
            _cachedData.AddRange(items);
        }
        finally
        {
            _lock.Release();
        }

        return this;
    }

    public async Task<bool> ExistsAsync(T item, bool shouldCache = true)
    {
        await _lock.WaitAsync();
        try
        {
            if (_cachedData.Contains(item)) return true;
            if (shouldCache)
                _cachedData.Add(item);

            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RemoveAsync(T item)
    {
        await _lock.WaitAsync();
        try
        {
            _cachedData.Remove(item);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}