using HealthAxis.API.Data;
using HealthAxis.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {

        protected readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<T?> AddAsync(T entity, CancellationToken ct = default)
        {
            await _context.Set<T>().AddAsync(entity, ct);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<T>().ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var existing =  await _context.Set<T>().FindAsync([id], ct);
            return existing;
        }

        public async Task<T?> UpdateAsync(int id, T entity, CancellationToken ct = default)
        {
            var existing = await _context.Set<T>().FindAsync([id], ct);

            if (existing is null) return null;

            _context.Entry(existing).CurrentValues.SetValues(entity);

            await _context.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<T?> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _context.Set<T>().FindAsync([id], ct);

            if (existing is null) return null;

            _context.Set<T>().Remove(existing);
            await _context.SaveChangesAsync(ct);

            return existing;
        }

    }
}
