using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static partial class ChampionshipMarkdown
{
    public static IHtmlContent Render(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return HtmlString.Empty;

        var html = new StringBuilder();
        var paragraph = new List<string>();
        string? list = null;

        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            var heading = Heading().Match(line);
            var bullet = Bullet().Match(line);
            var number = Numbered().Match(line);

            if (line.Trim().Length == 0)
            {
                Flush(html, paragraph);
                Close(html, ref list);

                continue;
            }

            if (heading.Success)
            {
                Flush(html, paragraph);
                Close(html, ref list);

                var level = Math.Min(heading.Groups[1].Value.Length + 2, 6);

                html.Append($"<h{level}>{Inline(heading.Groups[2].Value)}</h{level}>");

                continue;
            }

            if (bullet.Success || number.Success)
            {
                Flush(html, paragraph);

                var kind = bullet.Success ? "ul" : "ol";

                if (list != kind)
                {
                    Close(html, ref list);
                    html.Append($"<{kind}>");
                    list = kind;
                }

                html.Append($"<li>{Inline((bullet.Success ? bullet : number).Groups[1].Value)}</li>");

                continue;
            }

            if (line.Trim() is "---" or "***")
            {
                Flush(html, paragraph);
                Close(html, ref list);
                html.Append("<hr/>");

                continue;
            }

            Close(html, ref list);
            paragraph.Add(line.Trim());
        }

        Flush(html, paragraph);
        Close(html, ref list);

        return new HtmlString(html.ToString());
    }

    private static void Flush(StringBuilder html, List<string> paragraph)
    {
        if (paragraph.Count == 0)
            return;

        html.Append("<p>").Append(string.Join("<br/>", paragraph.Select(Inline))).Append("</p>");
        paragraph.Clear();
    }

    private static void Close(StringBuilder html, ref string? list)
    {
        if (list == null)
            return;

        html.Append($"</{list}>");
        list = null;
    }

    private static string Inline(string text)
    {
        var encoded = WebUtility.HtmlEncode(text);

        encoded = Link().Replace(encoded, match =>
        {
            var url = WebUtility.HtmlDecode(match.Groups[2].Value);

            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                ? $"<a href=\"{WebUtility.HtmlEncode(uri.ToString())}\" target=\"_blank\" rel=\"noopener nofollow\">{match.Groups[1].Value}</a>"
                : match.Groups[1].Value;
        });
        encoded = Bold().Replace(encoded, "<strong>$1</strong>");
        encoded = Italic().Replace(encoded, "<em>$1</em>");
        encoded = Code().Replace(encoded, "<code>$1</code>");

        return encoded;
    }

    [GeneratedRegex(@"^(#{1,4})\s+(.+)$")]
    private static partial Regex Heading();

    [GeneratedRegex(@"^\s*[-*]\s+(.+)$")]
    private static partial Regex Bullet();

    [GeneratedRegex(@"^\s*\d+[.)]\s+(.+)$")]
    private static partial Regex Numbered();

    [GeneratedRegex(@"\[([^\]]+)\]\(([^)\s]+)\)")]
    private static partial Regex Link();

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex Bold();

    [GeneratedRegex(@"(?<![*\w])\*([^*]+)\*(?![*\w])")]
    private static partial Regex Italic();

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex Code();
}
