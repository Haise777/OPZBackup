namespace Data.Contracts;

public interface IGenericRepository<T> where T : class
{
    Task CreateAsync(T dataToStore);
    Task CreateAsync(IEnumerable<T> datasToStore);
    
    Task<T> ReadSingleAsync(Func<T,bool> predicate);
    Task<IEnumerable<T>> ReadAsync(Func<T,bool> predicate);
    
    Task UpdateAsync(T dataToUpdate);
    Task UpdateAsync(IEnumerable<T> datasToUpdate);
    Task DeleteSingleAsync(Func<T, bool> predicate);
    Task DeleteRangeAsync(Func<T, bool> predicate);

    bool Exists(T dataToCheck);
    bool Exists(T dataToCheck, Func<T, bool> predicate);
}