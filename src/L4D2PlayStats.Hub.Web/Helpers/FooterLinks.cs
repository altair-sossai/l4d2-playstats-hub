using L4D2PlayStats.Hub.Web.Models;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class FooterLinks
{
    private static readonly IReadOnlyList<FooterLink> Default = [new(AppConsts.ProjectName, AppConsts.ProjectUrl)];

    public static IReadOnlyList<FooterLink> Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Default;

        var links = text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split(AppConsts.FooterLinkSeparator, 2, StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2 && parts[0].Length > 0 && IsWebAddress(parts[1]))
            .Select(parts => new FooterLink(parts[0], parts[1]))
            .ToList();

        return links.Count == 0 ? Default : links;
    }

    private static bool IsWebAddress(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
