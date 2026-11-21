using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static partial class CommandText
{
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

    public static IHtmlContent Format(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return HtmlString.Empty;

        var builder = new StringBuilder();
        var position = 0;

        foreach (Match match in Code().Matches(text))
        {
            builder.Append(Encode(text[position..match.Index]));
            builder.Append(AppConsts.CommandsCodeOpen).Append(Encode(match.Groups[1].Value)).Append(AppConsts.CommandsCodeClose);
            position = match.Index + match.Length;
        }

        builder.Append(Encode(text[position..]));

        return new HtmlString(builder.ToString());
    }

    private static string Encode(string value)
    {
        return Encoder.Encode(value);
    }

    [GeneratedRegex(AppConsts.CommandsCodePattern)]
    private static partial Regex Code();
}
