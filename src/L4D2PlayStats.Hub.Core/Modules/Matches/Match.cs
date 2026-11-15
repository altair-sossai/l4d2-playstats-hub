using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;

namespace L4D2PlayStats.Hub.Core.Modules.Matches;

public class Match
{
    public int Id { get; set; }
    public int GameServerId { get; set; }
    public GameServer? GameServer { get; set; }
    public int SeasonId { get; set; }
    public Season? Season { get; set; }
    public string? Campaign { get; set; }
    public string ConfigurationName { get; set; } = null!;
    public int TeamSize { get; set; }
    public DateTime FirstFileDate { get; set; }
    public DateTime LastFileDate { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public int TeamAScore { get; set; }
    public int TeamBScore { get; set; }
    public int MapCount { get; set; }
    public bool CountsForRanking { get; set; }
    public List<MatchPlayer> Players { get; set; } = [];
    public List<StatisticsFile> Files { get; set; } = [];
}

public class MatchPlayer : MatchPlayerStatistics
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match? Match { get; set; }
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public int Order { get; set; }
    public bool TeamA { get; set; }
    public bool TeamB { get; set; }
    public bool Win { get; set; }
    public bool Loss { get; set; }
    public int Draws { get; set; }
    public int RageQuits { get; set; }
    public bool Participant { get; set; }

    public int Teams => (TeamA ? 1 : 0) + (TeamB ? 1 : 0);
}

public class MatchPlayerStatistics
{
    public int Died { get; set; }
    public int Incaps { get; set; }
    public int DmgTaken { get; set; }
    public int Common { get; set; }
    public int SiKilled { get; set; }
    public int SiDamage { get; set; }
    public int TankDamage { get; set; }
    public int RockEats { get; set; }
    public int WitchDamage { get; set; }
    public int Skeets { get; set; }
    public int SkeetsMelee { get; set; }
    public int Levels { get; set; }
    public int Crowns { get; set; }
    public int FfGiven { get; set; }
    public int DmgTotal { get; set; }
    public int DmgTank { get; set; }
    public int DmgSpit { get; set; }
    public int HunterDpDmg { get; set; }
    public int MvpSiDamage { get; set; }
    public int MvpCommon { get; set; }
    public int LvpFfGiven { get; set; }

    public void Add(MatchPlayerStatistics other, int times = 1)
    {
        Died += other.Died * times;
        Incaps += other.Incaps * times;
        DmgTaken += other.DmgTaken * times;
        Common += other.Common * times;
        SiKilled += other.SiKilled * times;
        SiDamage += other.SiDamage * times;
        TankDamage += other.TankDamage * times;
        RockEats += other.RockEats * times;
        WitchDamage += other.WitchDamage * times;
        Skeets += other.Skeets * times;
        SkeetsMelee += other.SkeetsMelee * times;
        Levels += other.Levels * times;
        Crowns += other.Crowns * times;
        FfGiven += other.FfGiven * times;
        DmgTotal += other.DmgTotal * times;
        DmgTank += other.DmgTank * times;
        DmgSpit += other.DmgSpit * times;
        HunterDpDmg += other.HunterDpDmg * times;
        MvpSiDamage += other.MvpSiDamage * times;
        MvpCommon += other.MvpCommon * times;
        LvpFfGiven += other.LvpFfGiven * times;
    }
}
