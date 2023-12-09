namespace Data;

public interface IGenericRepository<T> where T : class
{
    Task Create(T dataToStore);
    
    Task<T> ReadSingleAsync(uint id);
    Task<IEnumerable<T>> ReadAsync(Func<T,bool> predicate);
    
    Task Update(T dataToUpdate);
    Task Delete(uint id);

    bool Exists(T dataToCheck);
    bool Exists(T dataToCheck, Func<T, bool> predicate);
}