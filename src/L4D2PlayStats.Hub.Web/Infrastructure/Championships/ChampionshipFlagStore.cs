using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Championships;

public class ChampionshipFlagStore(HubPaths paths)
{
    private readonly string _directory = Path.Combine(paths.DataDirectory, AppConsts.ChampionshipFlagsFolder);

    public static string? Extension(byte[] content)
    {
        if (content.Length >= 8 && content[0] == 0x89 && content[1] == 0x50 && content[2] == 0x4E && content[3] == 0x47)
            return ".png";

        if (content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
            return ".jpg";

        if (content.Length >= 6 && content[0] == 0x47 && content[1] == 0x49 && content[2] == 0x46 && content[3] == 0x38)
            return ".gif";

        if (content.Length >= 12 && content[0] == 0x52 && content[1] == 0x49 && content[2] == 0x46 && content[3] == 0x46 && content[8] == 0x57 && content[9] == 0x45 && content[10] == 0x42 && content[11] == 0x50)
            return ".webp";

        return null;
    }

    public async Task<string> SaveAsync(IFormFile upload, CancellationToken cancellationToken)
    {
        if (upload.Length > AppConsts.ChampionshipFlagMaxBytes)
            throw new ChampionshipException(AppConsts.ChampionshipFlagTooLargeMessage);

        using var memory = new MemoryStream();

        await upload.CopyToAsync(memory, cancellationToken);

        var content = memory.ToArray();
        var extension = Extension(content) ?? throw new ChampionshipException(AppConsts.ChampionshipFlagUnsupportedMessage);

        return await SaveAsync(content, extension, cancellationToken);
    }

    public async Task<string> SaveAsync(byte[] content, string extension, CancellationToken cancellationToken)
    {
        var name = $"{Guid.NewGuid():N}{extension}";

        Directory.CreateDirectory(_directory);
        await File.WriteAllBytesAsync(Path.Combine(_directory, name), content, cancellationToken);

        return name;
    }

    public string? PathOf(string? name)
    {
        if (string.IsNullOrEmpty(name) || name != Path.GetFileName(name))
            return null;

        var path = Path.Combine(_directory, name);

        return File.Exists(path) ? path : null;
    }

    public static string ContentType(string name)
    {
        return AppConsts.LogoContentTypes.GetValueOrDefault(Path.GetExtension(name).ToLowerInvariant(), "application/octet-stream");
    }

    public void Delete(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var path = PathOf(name);

            if (path != null)
                File.Delete(path);
        }
    }
}
