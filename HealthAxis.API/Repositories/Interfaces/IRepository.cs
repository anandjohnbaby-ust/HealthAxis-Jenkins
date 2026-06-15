namespace HealthAxis.API.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<T?> AddAsync(T entity, CancellationToken ct = default);

        Task<T?> UpdateAsync(int id, T entity, CancellationToken ct = default);

        Task<T?> DeleteAsync(int id, CancellationToken ct = default);
    }
}
