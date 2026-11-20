namespace L4D2PlayStats.Hub.Web.Infrastructure.Branding;

public class BrandingStore(HubPaths paths)
{
    private readonly string _directory = paths.Branding;
    private readonly Lock _lock = new();
    private bool _loaded;
    private string? _logoPath;

    public bool HasLogo => LogoPath != null;

    public string? LogoPath
    {
        get
        {
            lock (_lock)
            {
                if (_loaded)
                    return _logoPath;

                _logoPath = Find();
                _loaded = true;

                return _logoPath;
            }
        }
    }

    public long LogoVersion => LogoPath == null ? 0 : File.GetLastWriteTimeUtc(LogoPath).Ticks;

    public string? LogoContentType => LogoPath == null ? null : AppConsts.LogoContentTypes.GetValueOrDefault(Path.GetExtension(LogoPath).ToLowerInvariant());

    public static bool IsSupported(string? fileName)
    {
        return !string.IsNullOrEmpty(fileName) && AppConsts.LogoContentTypes.ContainsKey(Path.GetExtension(fileName).ToLowerInvariant());
    }

    public async Task SaveLogoAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var path = Path.Combine(_directory, $"{AppConsts.LogoFile}{extension}");

        RemoveLogo();
        Directory.CreateDirectory(_directory);

        await using (var file = File.Create(path))
        {
            await content.CopyToAsync(file, cancellationToken);
        }

        Reset();
    }

    public void RemoveLogo()
    {
        if (Directory.Exists(_directory))
        {
            foreach (var file in Directory.EnumerateFiles(_directory, $"{AppConsts.LogoFile}.*"))
                File.Delete(file);
        }

        Reset();
    }

    private void Reset()
    {
        lock (_lock)
        {
            _loaded = false;
        }
    }

    private string? Find()
    {
        if (!Directory.Exists(_directory))
            return null;

        return Directory
            .EnumerateFiles(_directory, $"{AppConsts.LogoFile}.*")
            .FirstOrDefault(file => AppConsts.LogoContentTypes.ContainsKey(Path.GetExtension(file).ToLowerInvariant()));
    }
}