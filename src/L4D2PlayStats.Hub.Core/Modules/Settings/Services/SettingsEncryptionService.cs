using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services;

public class SettingsEncryptionService(HubDbContext dbContext, IMemoryCache memoryCache, ISecretProtector secretProtector) : ISettingsEncryptionService
{
    public async Task<int> EncryptPlainSecretsAsync(CancellationToken cancellationToken = default)
    {
        if (!secretProtector.IsAvailable)
            return 0;

        var types = SecretProperties.SettingTypes().ToDictionary(type => type.Name);
        var keys = types.Keys.ToList();
        var settings = await dbContext.Settings.Where(setting => keys.Contains(setting.Key)).ToListAsync(cancellationToken);
        var encrypted = 0;

        foreach (var setting in settings)
        {
            var value = secretProtector.EncryptPlain(types[setting.Key], setting.Value);

            if (value == null)
                continue;

            setting.Value = value;
            encrypted++;
        }

        if (encrypted == 0)
            return 0;

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var setting in settings)
            memoryCache.Remove(string.Format(AppConsts.SettingsCacheKeyFormat, setting.Key));

        return encrypted;
    }
}
