namespace OPZBot.DataAccess.Caching;

public interface IDataCache<T>
{
    Task<IDataCache<T>> AddAsync(T item);
    Task<IDataCache<T>> AddRangeAsync(IEnumerable<T> items);
    Task<IDataCache<T>> UpdateRangeAsync(IEnumerable<T> items);
    Task<bool> ExistsAsync(T item, bool shouldCache = true);
    Task RemoveAsync(T item);
}