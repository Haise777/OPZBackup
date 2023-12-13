namespace OPZBot.DataAccess.Caching;

public interface IDataCache<T>
{
    Task<DataCache<T>> AddAsync(T item);
    Task<DataCache<T>> AddAsync(IEnumerable<T> items);
    Task<bool> ExistsAsync(T item, bool shouldCache = true);
    Task RemoveAsync(T item);
}