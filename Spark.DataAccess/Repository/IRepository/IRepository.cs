using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        // Async versions of the methods
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            CancellationToken cancellationToken = default);

        Task<T?> GetAsync(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null,
            bool tracked = false,
            CancellationToken cancellationToken = default);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        Task RemoveAsync(T entity, CancellationToken cancellationToken = default);

        Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    }
}
