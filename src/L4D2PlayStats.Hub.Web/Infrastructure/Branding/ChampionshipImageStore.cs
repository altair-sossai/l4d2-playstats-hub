using System.Globalization;
using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Branding;

public partial class ChampionshipImageStore(HubPaths paths)
{
    private readonly string _directory = Path.Combine(paths.Branding, AppConsts.ChampionshipImagesFolder);

    public static bool IsSupported(string? fileName)
    {
        return BrandingStore.IsSupported(fileName);
    }

    public static string? ContentType(string? fileName)
    {
        return string.IsNullOrEmpty(fileName) ? null : AppConsts.LogoContentTypes.GetValueOrDefault(Path.GetExtension(fileName).ToLowerInvariant());
    }

    public static bool IsValidName(string? fileName)
    {
        return !string.IsNullOrEmpty(fileName) && FileName().IsMatch(fileName) && ContentType(fileName) != null;
    }

    public static bool IsValidKind(string kind)
    {
        return Kind().IsMatch(kind);
    }

    public async Task<string> SaveAsync(int championshipId, string kind, Stream content, string extension, CancellationToken cancellationToken)
    {
        if (!IsValidKind(kind))
            throw new ArgumentException("The kind must have only lowercase letters, numbers and hyphens.", nameof(kind));

        var name = $"{kind}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        if (!IsValidName(name))
            throw new ArgumentException("The extension is not of a supported image.", nameof(extension));

        var directory = Folder(championshipId);

        Directory.CreateDirectory(directory);

        await using var file = File.Create(Path.Combine(directory, name));

        await content.CopyToAsync(file, cancellationToken);

        return name;
    }

    public string? PathOf(int championshipId, string? fileName)
    {
        if (!IsValidName(fileName))
            return null;

        var path = Path.Combine(Folder(championshipId), fileName!);

        return File.Exists(path) ? path : null;
    }

    public void Delete(int championshipId, string? fileName)
    {
        var path = PathOf(championshipId, fileName);

        if (path != null)
            File.Delete(path);
    }

    public void DeleteAll(int championshipId)
    {
        var directory = Folder(championshipId);

        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }

    private string Folder(int championshipId)
    {
        return Path.Combine(_directory, championshipId.ToString(CultureInfo.InvariantCulture));
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex Kind();

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*\.[a-z]{3,4}$")]
    private static partial Regex FileName();
}
