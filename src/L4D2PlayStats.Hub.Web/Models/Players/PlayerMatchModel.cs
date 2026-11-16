using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerMatchModel
{
    private readonly Match _match;
    private readonly DateTime _today;

    public PlayerMatchModel(PlayerMatch playerMatch, DateTime today)
    {
        var players = playerMatch.Match.Players.OrderBy(player => player.Order).ToList();

        _match = playerMatch.Match;
        _today = today.Date;

        Maps = playerMatch.Maps;

        Teams =
        [
            new PlayerMatchTeamModel(AppConsts.PlayerMatchTeamA, _match.TeamAScore, players.Where(player => player.TeamA)),
            new PlayerMatchTeamModel(AppConsts.PlayerMatchTeamB, _match.TeamBScore, players.Where(player => player.TeamB))
        ];
    }

    public int Id => _match.Id;
    public string? Campaign => _match.Campaign;
    public string ConfigurationName => _match.ConfigurationName;
    public bool Competitive => _match.CountsForRanking;
    public DateTime MatchStart => _match.StartedAtUtc ?? _match.FirstFileDate;
    public TimeSpan? MatchElapsed => _match.EndedAtUtc - MatchStart;
    public bool StartedToday => MatchStart.Date == _today;
    public bool StartedYesterday => MatchStart.Date == _today.AddDays(-1);
    public IReadOnlyList<PlayerMatchTeamModel> Teams { get; }
    public IReadOnlyList<PlayerMatchMap> Maps { get; }
    public PlayerMatchTeamModel Winner => Teams.MaxBy(team => team.Score)!;
    public int Diff => Math.Abs(_match.TeamAScore - _match.TeamBScore);
    public int? SeasonId { get; init; }

    public bool Includes(long communityId)
    {
        return Teams.Any(team => team.Players.Any(player => player.CommunityId == communityId));
    }
}
