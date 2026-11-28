using System.Globalization;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ChampionshipBadges
{
    private const int GoldenAngle = 137;

    public static string Hue(int seed)
    {
        return (Math.Abs((long)seed * GoldenAngle) % AppConsts.ChampionshipHueSteps).ToString(CultureInfo.InvariantCulture);
    }

    public static string Initials(string? name, string? tag = null)
    {
        if (!string.IsNullOrWhiteSpace(tag))
            return tag.Trim().ToUpperInvariant();

        var words = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(word => char.IsLetterOrDigit(word[0])).ToList();

        return words.Count switch
        {
            0 => "?",
            1 => new string(words[0].Where(char.IsLetterOrDigit).Take(2).ToArray()).ToUpperInvariant(),
            _ => string.Concat(words.Take(3).Select(word => char.ToUpperInvariant(word[0])))
        };
    }

    public static string Host(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host[4..] : uri.Host : url;
    }
}
