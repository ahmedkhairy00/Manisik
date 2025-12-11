using System.Linq.Expressions;
using UmarahBooking.Core.Const;

namespace UmarahBooking.Core.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);

        Task<IEnumerable<T>> FindWithAsync(string[] includes = null);
        Task<IEnumerable<T>> FindWithAsync(Expression<Func<T, bool>> predicate, string[] includes = null);
        Task<T?> FindBySearch(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAllBySearch(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAllBySearchAndSkip(Expression<Func<T, bool>> predicate, int? take, int? skip);
        Task<IEnumerable<T>> FindAllBySearchAndSkipWithOrder(
            Expression<Func<T, bool>> criteria,
            int? take,
            int? skip,
            Expression<Func<T, object>> orderBy = null,
            string orderByDirection = OrderBy.Ascending
        );
        IQueryable<T> GetAllAsQuerable();
    }
}
