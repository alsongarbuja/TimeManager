using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace TimeManager.Backend.Extensions
{
    public static class DbSetExtensions
    {
        public static async Task<T> FindOrThrowAsync<T>(this DbSet<T> dbSet, int id) where T : class
        {
            var entity = await dbSet.FindAsync(id);
            return entity ?? throw new KeyNotFoundException($"{typeof(T).Name} was not found");
        }

        public static async Task<T> WhereOrThrowAsync<T>(this DbSet<T> dbSet, Expression<System.Func<T, bool>> query) where T : class
        {
            var entity = await dbSet.Where(query).FirstOrDefaultAsync();
            return entity ?? throw new KeyNotFoundException($"{typeof(T).Name} for the given query was not found");
        }

        public static async Task<(List<TResult>, int)> FindWithPaginationAsync<TEntity, TResult>(
            this DbSet<TEntity> dbSet, 
            Expression<Func<TEntity, TResult>> selector,
            int skip, 
            int take,
            Expression<Func<TEntity, bool>>? where = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool isDescending = false
            ) where TEntity : class
        {
            IQueryable<TEntity> query = dbSet.AsNoTracking().AsQueryable();
            int totalRecords;
            if (where == null)
            {
                totalRecords = await query.CountAsync();
            } else
            {
                totalRecords = await query.Where(where).CountAsync();
                query = query.Where(where);
            }

            if (orderBy != null)
            {
                query = isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }

            List<TResult> result = await query.Skip(skip).Take(take).Select(selector).ToListAsync();

            return (result, totalRecords);
        }
    }
}
