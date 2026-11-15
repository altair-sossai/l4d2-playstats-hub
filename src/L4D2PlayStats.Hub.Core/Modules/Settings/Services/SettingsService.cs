using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services;

public class SettingsService(HubDbContext dbContext, IMemoryCache memoryCache, HubState hubState) : ISettingsService
{
    public async Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
    {
        var key = typeof(T).Name;

        var json = await memoryCache.GetOrCreateAsync(CacheKey(key), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConsts.SettingsCacheInMinutes);

            var setting = await dbContext.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

            return setting?.Value ?? string.Empty;
        });

        return string.IsNullOrEmpty(json) ? new T() : JsonSerializer.Deserialize<T>(json) ?? new T();
    }

    public async Task SaveAsync<T>(T settings, CancellationToken cancellationToken = default) where T : class, new()
    {
        var key = typeof(T).Name;
        var json = JsonSerializer.Serialize(settings);
        var setting = await dbContext.Settings.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        if (setting == null)
        {
            setting = new Setting { Key = key };
            dbContext.Settings.Add(setting);
        }

        setting.Value = json;

        await dbContext.SaveChangesAsync(cancellationToken);

        memoryCache.Remove(CacheKey(key));
        hubState.DataChanged();
    }

    private static string CacheKey(string key)
    {
        return $"settings_{key}";
    }
}
