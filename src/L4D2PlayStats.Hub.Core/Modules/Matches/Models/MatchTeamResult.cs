namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public class MatchTeamResult(char code, int score) : MatchPlayerStatistics
{
    private readonly List<MatchPlayerResult> _players = [];

    public char Code { get; } = code;
    public int Score { get; } = score;
    public IReadOnlyList<MatchPlayerResult> Players => _players;

    public MatchPlayerResult AddPlayer(long communityId, string? name, MatchPlayerStatistics? statistics)
    {
        var player = new MatchPlayerResult(this, communityId, name);

        if (statistics != null)
        {
            player.Add(statistics);
            Add(statistics);
        }

        _players.Add(player);

        return player;
    }
}
