namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;

public enum PlayerReplacementError
{
    SamePlayer,
    FileNotFound,
    UnreadableFile,
    NotInHalf,
    AlreadyInHalf,
    OtherTeam
}

// MatchId is the match the map belongs to after the rebuild, so the page can be opened again.
public record PlayerReplacementResult(PlayerReplacementError? Error, int? MatchId = null)
{
    public bool Replaced => Error == null;

    public static PlayerReplacementResult Failed(PlayerReplacementError error)
    {
        return new PlayerReplacementResult(error);
    }
}
