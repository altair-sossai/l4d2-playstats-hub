using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Tests.Support;

public static partial class PageScripts
{
    public static async Task<string> WithScriptsAsync(this HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var builder = new StringBuilder(html);

        foreach (Match match in Script().Matches(html))
            builder.AppendLine().Append(await client.GetStringAsync(match.Groups[1].Value));

        return builder.ToString();
    }

    public static JsonElement Config(string html, string name)
    {
        var match = Regex.Match(html, $"<script type=\"application/json\" id=\"{name}-config\">(.*?)</script>", RegexOptions.Singleline);

        return JsonDocument.Parse(match.Groups[1].Value).RootElement;
    }

    [GeneratedRegex("src=\"(/js/pages/[^\"?]+)")]
    private static partial Regex Script();
}
