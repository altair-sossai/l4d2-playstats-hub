namespace L4D2PlayStats.Hub.Core.Infrastructure.Helpers;

public static class SafeUrl
{
    public static bool IsHttps(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }

    public static bool IsHttp(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static string? Https(string? value)
    {
        var trimmed = value?.Trim();

        return IsHttps(trimmed) ? trimmed : null;
    }

    public static string? Http(string? value)
    {
        var trimmed = value?.Trim();

        return IsHttp(trimmed) ? trimmed : null;
    }
}
