namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public record MatchMapResult(int FileId, string MapName, int TeamAScore, int TeamBScore, long? TankWipeByTeamA = null, long? TankWipeByTeamB = null)
{
    public long? TankWipeBy(char team)
    {
        return team == AppConsts.TeamACode ? TankWipeByTeamA : TankWipeByTeamB;
    }
}
