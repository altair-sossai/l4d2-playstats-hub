using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services;

public class SettingsService(HubDbContext dbContext, IMemoryCache memoryCache, HubState hubState, ISecretProtector secretProtector) : ISettingsService
{
    public async Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
    {
        var key = typeof(T).Name;

        var json = await memoryCache.GetOrCreateAsync(CacheKey(key), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConsts.SettingsCacheInMinutes);

            var setting = await dbContext.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

            return setting == null ? string.Empty : secretProtector.Open(typeof(T), setting.Value);
        });

        return string.IsNullOrEmpty(json) ? new T() : JsonSerializer.Deserialize<T>(json) ?? new T();
    }

    public async Task SaveAsync<T>(T settings, CancellationToken cancellationToken = default) where T : class, new()
    {
        var key = typeof(T).Name;
        var json = JsonSerializer.Serialize(settings);
        var setting = await dbContext.Settings.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
        var stored = setting?.Value;

        if (setting == null)
        {
            setting = new Setting { Key = key };
            dbContext.Settings.Add(setting);
        }

        setting.Value = secretProtector.Seal(typeof(T), json, stored);

        await dbContext.SaveChangesAsync(cancellationToken);

        memoryCache.Remove(CacheKey(key));
        hubState.DataChanged();
    }

    private static string CacheKey(string key)
    {
        return string.Format(AppConsts.SettingsCacheKeyFormat, key);
    }
}