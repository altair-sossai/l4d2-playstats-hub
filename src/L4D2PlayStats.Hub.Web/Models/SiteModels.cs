using System.Globalization;
using System.Text;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Patents;
using L4D2PlayStats.Hub.Core.Modules.Ranking;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Models;

public enum RankingScope
{
    Season,
    Year,
    AllTime
}

public class RankingViewModel
{
    public RankingScope Scope { get; init; }
    public Season? Season { get; init; }
    public int? Year { get; init; }
    public bool Current { get; init; }
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public IReadOnlyList<int> Years { get; init; } = [];
    public IReadOnlyList<RankingRow> Players { get; init; } = [];
    public long? Highlight { get; init; }
}

public class RankingRow(RankingPlayer player)
{
    public RankingPlayer Player { get; } = player;
    public PatentProgress Progress { get; } = new(player.Experience, player.PreviousExperience);
}

public class MatchListViewModel
{
    public MatchPage Page { get; init; } = null!;
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public IReadOnlyList<GameServer> Servers { get; init; } = [];
    public int? SeasonId { get; init; }
    public int? ServerId { get; init; }
}

public class MatchViewModel
{
    public Match Match { get; init; } = null!;
    public IReadOnlyList<RankingPlayer> Ranking { get; init; } = [];
    public bool Administrator { get; init; }

    public IEnumerable<MatchPlayer> Team(char team)
    {
        return Match.Players.Where(p => team == 'A' ? p.TeamA : p.TeamB).OrderByDescending(p => p.SiDamage);
    }

    public IEnumerable<MatchPlayer> Others => Match.Players.Where(p => !p.TeamA && !p.TeamB && p.RageQuits > 0);
}

public class PlayerViewModel
{
    public long CommunityId { get; init; }
    public string Name { get; init; } = null!;
    public Season Season { get; init; } = null!;
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public RankingPlayer? Ranked { get; init; }
    public PatentProgress? Progress { get; init; }
    public RankingPlayer? AllTime { get; init; }
    public IReadOnlyList<Match> Matches { get; init; } = [];
    public IReadOnlyDictionary<long, string> Names { get; init; } = new Dictionary<long, string>();
    public int Stars { get; init; }
    public bool Administrator { get; init; }
}

public class PlayersViewModel
{
    public Season Season { get; init; } = null!;
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public IReadOnlyList<RankingPlayer> Players { get; init; } = [];
    public string OrderBy { get; init; } = null!;
}

public class InGameViewModel
{
    public string SiteName { get; init; } = null!;
    public Season Season { get; init; } = null!;
    public IReadOnlyList<RankingRow> Players { get; init; } = [];
    public RankingRow? Me { get; init; }
    public IReadOnlyList<Match> Matches { get; init; } = [];
    public long? SteamId { get; init; }
}

public static class Display
{
    public static string SeasonName(Season season)
    {
        var culture = CultureInfo.CurrentCulture;
        var last = season.EndsAt.AddDays(-1);
        var first = culture.TextInfo.ToTitleCase(season.StartsAt.ToString("MMM", culture).TrimEnd('.'));
        var final = culture.TextInfo.ToTitleCase(last.ToString("MMM", culture).TrimEnd('.'));

        if (season.StartsAt.Year == last.Year && season.StartsAt.Month == last.Month)
            return $"{first} {last.Year}";

        return season.StartsAt.Year == last.Year
            ? $"{first} – {final} {last.Year}"
            : $"{first} {season.StartsAt.Year} – {final} {last.Year}";
    }

    public static string CampaignImage(string? campaign)
    {
        if (string.IsNullOrEmpty(campaign))
            return "/imgs/campaigns/no-image.jpg";

        var builder = new StringBuilder();

        foreach (var character in campaign.ToLowerInvariant())
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
                builder.Append(character);
            else if (character is ' ' or '-' && builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        return $"/imgs/campaigns/{builder.ToString().Trim('-')}.jpg";
    }

    public static string Percent(decimal value)
    {
        return (value * 100).ToString("0", CultureInfo.CurrentCulture) + "%";
    }

    public static string Number(decimal value)
    {
        return value.ToString("N0", CultureInfo.CurrentCulture);
    }

    public static string PerGame(int value, int games)
    {
        return games == 0 ? "-" : (value / (decimal)games).ToString("N1", CultureInfo.CurrentCulture);
    }

    public static string Duration(DateTime? start, DateTime? end)
    {
        if (start == null || end == null || end < start)
            return "-";

        var elapsed = end.Value - start.Value;

        return elapsed.TotalHours >= 1 ? $"{(int)elapsed.TotalHours}h {elapsed.Minutes:00}m" : $"{elapsed.Minutes}m";
    }
}
