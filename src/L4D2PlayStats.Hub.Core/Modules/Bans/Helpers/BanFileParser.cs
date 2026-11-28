using System.Globalization;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Bans.Helpers;

public static class BanFileParser
{
    private static readonly Regex BanLine = new(AppConsts.BanFileLinePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static BanFile Parse(string? text)
    {
        var bans = new Dictionary<long, ImportedBan>();
        var invalid = new List<string>();
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var afterBan = false;

        foreach (var line in (text ?? string.Empty).Split('\n', StringSplitOptions.TrimEntries))
        {
            if (line.Length == 0)
            {
                if (afterBan)
                    fields.Clear();

                afterBan = false;
                continue;
            }

            if (line.StartsWith(AppConsts.BanCommentPrefix, StringComparison.Ordinal))
            {
                if (afterBan)
                    fields.Clear();

                afterBan = false;
                Comment(line, fields);
                continue;
            }

            var match = BanLine.Match(line);

            if (!match.Success)
                continue;

            afterBan = true;

            var communityId = SteamIdParser.Parse(match.Groups[2].Value);

            if (communityId == null || !int.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out var minutes))
            {
                invalid.Add(line);
                continue;
            }

            bans[communityId.Value] = new ImportedBan(
                communityId.Value,
                Text.Clip(Field(fields, AppConsts.BanFileNameField), AppConsts.LivePlayerNameMaxLength),
                Text.Clip(Field(fields, AppConsts.BanFileReasonField), AppConsts.BanReasonMaxLength),
                Date(Field(fields, AppConsts.BanFileDateField)),
                Date(Field(fields, AppConsts.BanFileExpireField)),
                minutes);
        }

        return new BanFile([.. bans.Values], invalid);
    }

    private static void Comment(string line, Dictionary<string, string> fields)
    {
        var content = line[AppConsts.BanCommentPrefix.Length..].Trim();
        var separator = content.IndexOf(':');

        if (separator <= 0)
            return;

        var key = content[..separator].Trim();

        if (key.Equals(AppConsts.BanFileNameField, StringComparison.OrdinalIgnoreCase))
            fields.Clear();

        fields[key] = content[(separator + 1)..].Trim();
    }

    private static string? Field(Dictionary<string, string> fields, string key)
    {
        return fields.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
    }

    private static DateTime? Date(string? value)
    {
        if (value == null || value.Equals(AppConsts.BanFileNever, StringComparison.OrdinalIgnoreCase))
            return null;

        return DateTime.TryParseExact(value, AppConsts.BanFileDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    }
}
