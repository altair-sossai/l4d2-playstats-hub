using L4D2PlayStats.Hub.Web.Models;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class FooterLinks
{
    private static readonly IReadOnlyList<FooterLink> Default = [new(AppConsts.ProjectName, AppConsts.ProjectUrl)];

    public static IReadOnlyList<FooterLink> Parse(string? text)
    {
        var links = Valid(text);

        return links.Count == 0 ? Default : links;
    }

    public static string? Normalize(string? text)
    {
        var links = Valid(text);

        return links.Count == 0 ? null : string.Join('\n', links.Select(link => $"{link.Text} {AppConsts.FooterLinkSeparator} {link.Url}"));
    }

    public static bool HasInvalidLines(string? text)
    {
        return Lines(text).Length > Valid(text).Count;
    }

    private static List<FooterLink> Valid(string? text)
    {
        return Lines(text)
            .Select(line => line.Split(AppConsts.FooterLinkSeparator, 2, StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2 && parts[0].Length > 0 && IsWebAddress(parts[1]))
            .Select(parts => new FooterLink(parts[0], parts[1]))
            .ToList();
    }

    private static string[] Lines(string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? [] : text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool IsWebAddress(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
