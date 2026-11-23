namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;

public enum PlayerReplacementError
{
    SamePlayer,
    NoMaps,
    MatchNotFound,
    MapNotInMatch,
    UnreadableMap,
    NotInMap,
    AlreadyInMap
}

// MatchId is the match the maps belong to after the rebuild; MapName names the map that stopped the replacement.
public record PlayerReplacementResult(PlayerReplacementError? Error, int? MatchId = null, string? MapName = null)
{
    public bool Replaced => Error == null;

    public static PlayerReplacementResult Failed(PlayerReplacementError error, string? mapName = null)
    {
        return new PlayerReplacementResult(error, MapName: mapName);
    }
}
