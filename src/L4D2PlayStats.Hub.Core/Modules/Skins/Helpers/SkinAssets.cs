using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinAssets
{
    public static string? BaseUrl(string? assetsUrl)
    {
        var trimmed = (string.IsNullOrWhiteSpace(assetsUrl) ? AppConsts.SkinDefaultAssetsUrl : assetsUrl).Trim().TrimEnd('/');

        return SafeUrl.IsHttp(trimmed) ? trimmed : null;
    }

    public static string? Thumbnail(string? assetsUrl, string skinId)
    {
        var baseUrl = BaseUrl(assetsUrl);

        return baseUrl == null ? null : $"{baseUrl}/{AppConsts.SkinThumbnailsFolder}/{Uri.EscapeDataString(skinId)}{AppConsts.SkinThumbnailExtension}";
    }

    public static string? ShotsFolder(string? assetsUrl, string skinId)
    {
        var baseUrl = BaseUrl(assetsUrl);

        return baseUrl == null ? null : $"{baseUrl}/{AppConsts.SkinShotsFolder}/{Uri.EscapeDataString(skinId)}/";
    }
}
