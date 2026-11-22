namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public class MatchPlayerResult(MatchTeamResult team, long communityId, string? name) : MatchPlayerStatistics
{
    public long CommunityId { get; } = communityId;
    public string? Name { get; } = name;

    public decimal DiedPercentage => Share(Died, team.Died);
    public decimal IncapsPercentage => Share(Incaps, team.Incaps);
    public decimal DmgTakenPercentage => Share(DmgTaken, team.DmgTaken);
    public decimal CommonPercentage => Share(Common, team.Common);
    public decimal SiKilledPercentage => Share(SiKilled, team.SiKilled);
    public decimal SiDamagePercentage => Share(SiDamage, team.SiDamage);
    public decimal TankDamagePercentage => Share(TankDamage, team.TankDamage);
    public decimal RockEatsPercentage => Share(RockEats, team.RockEats);
    public decimal WitchDamagePercentage => Share(WitchDamage, team.WitchDamage);
    public decimal SkeetsPercentage => Share(Skeets, team.Skeets);
    public decimal SkeetsMeleePercentage => Share(SkeetsMelee, team.SkeetsMelee);
    public decimal LevelsPercentage => Share(Levels, team.Levels);
    public decimal CrownsPercentage => Share(Crowns, team.Crowns);
    public decimal FfGivenPercentage => Share(FfGiven, team.FfGiven);

    public decimal DmgTotalPercentage => Share(DmgTotal, team.DmgTotal);
    public decimal DmgTankPercentage => Share(DmgTank, team.DmgTank);
    public decimal DmgSpitPercentage => Share(DmgSpit, team.DmgSpit);
    public decimal HunterDpDmgPercentage => Share(HunterDpDmg, team.HunterDpDmg);

    public decimal MvpSiDamagePercentage => Share(MvpSiDamage, team.MvpSiDamage);
    public decimal MvpCommonPercentage => Share(MvpCommon, team.MvpCommon);
    public int TotalMvp => MvpSiDamage + MvpCommon;
    public decimal LvpFfGivenPercentage => Share(LvpFfGiven, team.LvpFfGiven);
    public decimal TankWipesPercentage => Share(TankWipes, team.TankWipes);

    private static decimal Share(decimal value, decimal total)
    {
        return total == 0 ? 0 : value / total;
    }
}
