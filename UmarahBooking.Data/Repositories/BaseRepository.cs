using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UmarahBooking.Core.Const;
using UmarahBooking.Core.Interfaces;
using UmarahBooking.Data.DatabaseContext;

namespace UmarahBooking.Data.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        public BaseRepository(ApplicationDbContext context) => _context = context;

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _context.Set<T>().AddAsync(entity);
            // SaveChanges removed to centralize commits in UnitOfWork
            return entity;
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            // SaveChanges removed to centralize commits in UnitOfWork
        }

        public async Task<IEnumerable<T>> FindAllBySearch(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllBySearchAndSkip(
            Expression<Func<T, bool>> predicate, int? take, int? skip)
        {
            var query = _context.Set<T>().Where(predicate);
            if (skip.HasValue) query = query.Skip(skip.Value);
            if (take.HasValue) query = query.Take(take.Value);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllBySearchAndSkipWithOrder(
            Expression<Func<T, bool>> criteria,
            int? take,
            int? skip,
            Expression<Func<T, object>>? orderBy = null,
            string orderByDirection = OrderBy.Ascending)
        {
            var query = _context.Set<T>().Where(criteria);

            if (orderBy != null)
                query = orderByDirection == OrderBy.Ascending
                    ? query.OrderBy(orderBy)
                    : query.OrderByDescending(orderBy);

            if (skip.HasValue) query = query.Skip(skip.Value);
            if (take.HasValue) query = query.Take(take.Value);

            return await query.ToListAsync();
        }

        public async Task<T?> FindBySearch(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> FindWithAsync(string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindWithAsync(Expression<Func<T, bool>> predicate, string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();

        public IQueryable<T> GetAllAsQuerable() => _context.Set<T>().AsNoTracking();

        public async Task<T> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("ID must be greater than zero.", nameof(id));
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            // SaveChanges removed to centralize commits in UnitOfWork
            return entity;
        }
    }
}
