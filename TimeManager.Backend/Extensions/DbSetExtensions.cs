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
    }
}
