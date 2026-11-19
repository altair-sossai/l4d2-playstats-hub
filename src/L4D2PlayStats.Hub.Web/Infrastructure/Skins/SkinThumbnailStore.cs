using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Skins;

public class SkinThumbnailStore(IWebHostEnvironment environment)
{
    private readonly Lazy<HashSet<string>> _bundled = new(() => Find(environment));

    public IReadOnlyCollection<string> Bundled => _bundled.Value;

    public bool IsBundled(string skinId)
    {
        return _bundled.Value.Contains(skinId);
    }

    public string? Url(string? assetsUrl, string skinId)
    {
        return IsBundled(skinId) ? $"/{AppConsts.SkinBundledThumbnailsPath}/{Uri.EscapeDataString(skinId)}{CoreConsts.SkinThumbnailExtension}" : SkinAssets.Thumbnail(assetsUrl, skinId);
    }

    public string Image(string? assetsUrl, Skin skin)
    {
        return Url(assetsUrl, skin.Id) ?? AppConsts.SkinEmptyImage;
    }

    private static HashSet<string> Find(IWebHostEnvironment environment)
    {
        return environment.WebRootFileProvider
            .GetDirectoryContents(AppConsts.SkinBundledThumbnailsPath)
            .Where(file => !file.IsDirectory && file.Name.EndsWith(CoreConsts.SkinThumbnailExtension, StringComparison.Ordinal))
            .Select(file => file.Name[..^CoreConsts.SkinThumbnailExtension.Length])
            .ToHashSet(StringComparer.Ordinal);
    }
}
