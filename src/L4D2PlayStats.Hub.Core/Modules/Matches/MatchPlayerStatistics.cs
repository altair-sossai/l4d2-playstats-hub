namespace L4D2PlayStats.Hub.Core.Modules.Matches;

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
    public int TankWipes { get; set; }

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
        TankWipes += other.TankWipes * times;
    }
}
