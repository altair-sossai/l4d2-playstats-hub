using System.Reflection;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinBundledCatalog
{
    private static readonly Lazy<string> Content = new(Load);
    private static readonly Lazy<int> SkinCount = new(() => SkinCatalogReader.Read(Content.Value)?.Count ?? 0);

    public static string Json => Content.Value;

    public static int Count => SkinCount.Value;

    private static string Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceNames().First(name => name.EndsWith(AppConsts.SkinBundledCatalogResource, StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
