namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinAssets
{
    public static string? BaseUrl(string? assetsUrl)
    {
        var trimmed = (string.IsNullOrWhiteSpace(assetsUrl) ? AppConsts.SkinDefaultAssetsUrl : assetsUrl).Trim().TrimEnd('/');

        return IsHttpUrl(trimmed) ? trimmed : null;
    }

    public static bool IsHttpUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
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
