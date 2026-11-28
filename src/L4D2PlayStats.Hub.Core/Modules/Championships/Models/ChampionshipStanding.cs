namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public class ChampionshipStanding
{
    public int TeamId { get; init; }
    public int Seed { get; init; }
    public int Position { get; set; }
    public int Played { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GameWins { get; set; }
    public int GameLosses { get; set; }
    public int ScoreFor { get; set; }
    public int ScoreAgainst { get; set; }
    public int Penalties { get; set; }
    public int PenaltyPoints { get; set; }
    public int MatchPoints { get; set; }

    public int Points => MatchPoints - PenaltyPoints;

    public int Difference => ScoreFor - ScoreAgainst - Penalties;
}
