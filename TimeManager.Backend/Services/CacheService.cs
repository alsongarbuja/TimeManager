using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TimeManager.Backend.Data;
using TimeManager.Backend.Models;

namespace TimeManager.Backend.Services
{
    public interface ICacheService
    {
        public Task<Preferences> GetPreferencesAsync(int userId);
        public Task UpdatePreferencesAsync(int userId, Preferences newPrefs);
    }

    public class CacheService(HrmsDbContext context, IMemoryCache cache): ICacheService
    {
        public async Task<Preferences> GetPreferencesAsync(int userId)
        {
            string cacheKey = $"Prefs_{userId}";

            if (cache.TryGetValue(cacheKey, out Preferences cachedPrefs))
            {
                return cachedPrefs;
            }

            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            var prefs = user?.Preferences ?? new Preferences();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(10));
            cache.Set(cacheKey, prefs, cacheOptions);

            return prefs;
        }

        public async Task UpdatePreferencesAsync(int userId, Preferences newPrefs)
        {
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Preferences = newPrefs;
                await context.SaveChangesAsync();
            }

            string cacheKey = $"Prefs_{userId}";
            cache.Set(cacheKey, newPrefs, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(10)));
        }
    }
}
