namespace OPZBot.DataAccess.Caching;

public interface IDataCache<T>
{
    Task<DataCache<T>> AddAsync(T item);
    Task<DataCache<T>> AddRangeAsync(IEnumerable<T> items);
    Task<DataCache<T>> UpdateRangeAsync(IEnumerable<T> items);
    Task<bool> ExistsAsync(T item, bool shouldCache = true);
    Task RemoveAsync(T item);
}