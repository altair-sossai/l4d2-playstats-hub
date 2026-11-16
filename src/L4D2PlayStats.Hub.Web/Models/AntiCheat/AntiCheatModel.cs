using System.Globalization;

namespace L4D2PlayStats.Hub.Web.Models.AntiCheat;

public class AntiCheatModel(ClientVersionResponse? latest, string? fallbackDownloadUrl)
{
    public string? Version { get; init; } = latest?.Version;
    public string? DownloadUrl { get; init; } = IsHttps(latest?.DownloadUrl) ? latest!.DownloadUrl : IsHttps(fallbackDownloadUrl) ? fallbackDownloadUrl : null;
    public string? Sha256 { get; init; } = latest?.Sha256;
    public long? SizeBytes { get; init; } = latest?.SizeBytes;
    public DateTimeOffset? PublishedAt { get; init; } = latest?.PublishedAt;
    public string? SizeText => SizeBytes is > 0 ? (SizeBytes.Value / AppConsts.BytesPerMegabyte).ToString("0.0", CultureInfo.CurrentCulture) + " MB" : null;

    private static bool IsHttps(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }
}
