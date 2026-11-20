using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using Microsoft.Extensions.Caching.Memory;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class AntiCheatVersionService(
    ISettingsService settingsService,
    IHttpClientFactory httpClientFactory,
    IMemoryCache memoryCache,
    ILogger<AntiCheatVersionService> logger) : IAntiCheatVersionService
{
    public async Task<ClientVersionResponse?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<AntiCheatSettings>(cancellationToken);

        if (!Uri.TryCreate(settings.ServiceUrl, UriKind.Absolute, out var service) || service.Scheme != Uri.UriSchemeHttps)
            return null;

        var address = $"{settings.ServiceUrl!.TrimEnd('/')}{AppConsts.AntiCheatVersionPath}";

        return await memoryCache.GetOrCreateAsync($"{AppConsts.AntiCheatVersionCacheKey}:{address}", async entry =>
        {
            var response = await FetchAsync(address, cancellationToken);

            entry.AbsoluteExpirationRelativeToNow = response == null
                ? TimeSpan.FromSeconds(AppConsts.AntiCheatFailureCacheInSeconds)
                : TimeSpan.FromMinutes(AppConsts.AntiCheatVersionCacheInMinutes);

            return response;
        });
    }

    private async Task<ClientVersionResponse?> FetchAsync(string address, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(AppConsts.AntiCheatTimeoutInSeconds);

            var response = await client.GetFromJsonAsync<ClientVersionResponse>(address, cancellationToken);

            return string.IsNullOrWhiteSpace(response?.Version) ? null : response;
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Failed to get the latest Anti-Cheat version");

            return null;
        }
    }
}
