using Microsoft.EntityFrameworkCore;

namespace TimeManager.Backend.Extensions
{
    public static class DbSetExtensions
    {
        public static async Task<T> FindOrThrowAsync<T>(this DbSet<T> dbSet, int id) where T : class
        {
            var entity = await dbSet.FindAsync(id);
            return entity ?? throw new KeyNotFoundException($"{typeof(T).Name} was not found");
        }
    }
}
