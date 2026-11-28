using System.Globalization;
using System.Text;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class ChampionshipSlugs
{
    public static string From(string? text)
    {
        var builder = new StringBuilder();

        foreach (var character in (text ?? string.Empty).Normalize(NormalizationForm.FormD).ToLowerInvariant())
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
                builder.Append(character);
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        var slug = builder.ToString().Trim('-');

        if (slug.Length > AppConsts.ChampionshipSlugMaxLength)
            slug = slug[..AppConsts.ChampionshipSlugMaxLength].Trim('-');

        return slug.Length == 0 ? "championship" : slug;
    }
}
