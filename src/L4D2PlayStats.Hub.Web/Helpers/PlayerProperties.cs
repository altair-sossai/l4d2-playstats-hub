using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class PlayerProperties
{
    public static decimal Value(RankingPlayer player, PlayerResultProperty property)
    {
        return property switch
        {
            PlayerResultProperty.Experience => player.Experience,
            PlayerResultProperty.Games => player.Games,
            PlayerResultProperty.Wins => player.Wins,
            PlayerResultProperty.Loss => player.Loss,
            PlayerResultProperty.WinRate => player.WinRate,
            PlayerResultProperty.RageQuit => player.RageQuit,
            PlayerResultProperty.MvpSiDamage => player.MvpSiDamage,
            PlayerResultProperty.MvpCommon => player.MvpCommon,
            PlayerResultProperty.LvpFfGiven => player.LvpFfGiven,
            PlayerResultProperty.Died => player.Died,
            PlayerResultProperty.Incaps => player.Incaps,
            PlayerResultProperty.DmgTaken => player.DmgTaken,
            PlayerResultProperty.Common => player.Common,
            PlayerResultProperty.SiKilled => player.SiKilled,
            PlayerResultProperty.SiDamage => player.SiDamage,
            PlayerResultProperty.TankDamage => player.TankDamage,
            PlayerResultProperty.RockEats => player.RockEats,
            PlayerResultProperty.Skeets => player.Skeets,
            PlayerResultProperty.SkeetsMelee => player.SkeetsMelee,
            PlayerResultProperty.Levels => player.Levels,
            PlayerResultProperty.Crowns => player.Crowns,
            PlayerResultProperty.FfGiven => player.FfGiven,
            PlayerResultProperty.DmgTotal => player.DmgTotal,
            PlayerResultProperty.DmgTank => player.DmgTank,
            PlayerResultProperty.DmgSpit => player.DmgSpit,
            PlayerResultProperty.HunterDpDmg => player.HunterDpDmg,
            _ => 0
        };
    }

    public static string Title(PlayerResultProperty property)
    {
        return property switch
        {
            PlayerResultProperty.Experience => "Experience",
            PlayerResultProperty.Games => "Games",
            PlayerResultProperty.Wins => "Wins",
            PlayerResultProperty.Loss => "Loss",
            PlayerResultProperty.WinRate => "Win rate",
            PlayerResultProperty.RageQuit => "Rage quit",
            PlayerResultProperty.MvpSiDamage => "SI MVP awards",
            PlayerResultProperty.MvpCommon => "CI MVP awards",
            PlayerResultProperty.LvpFfGiven => "FF LVP awards",
            PlayerResultProperty.Died => "Deaths",
            PlayerResultProperty.Incaps => "Times incapacitated",
            PlayerResultProperty.DmgTaken => "SI damage taken",
            PlayerResultProperty.Common => "Commons killed",
            PlayerResultProperty.SiKilled => "SI killed",
            PlayerResultProperty.SiDamage => "Damage to SI",
            PlayerResultProperty.TankDamage => "Damage to Tank",
            PlayerResultProperty.RockEats => "Rock hits taken",
            PlayerResultProperty.Skeets => "Total skeets",
            PlayerResultProperty.SkeetsMelee => "Melee skeets",
            PlayerResultProperty.Levels => "Total levels",
            PlayerResultProperty.Crowns => "Total crowns",
            PlayerResultProperty.FfGiven => "FF dealt (non-incap.)",
            PlayerResultProperty.DmgTotal => "SI damage (excl. Tank)",
            PlayerResultProperty.DmgTank => "Damage as Tank (upright)",
            PlayerResultProperty.DmgSpit => "Spit damage (upright)",
            PlayerResultProperty.HunterDpDmg => "Total pounce damage",
            _ => property.ToString()
        };
    }

    public static string? Help(PlayerResultProperty property)
    {
        return property switch
        {
            PlayerResultProperty.DmgTaken => "Damage from player infected and commons attributed to a Boomer; regular common damage is excluded.",
            PlayerResultProperty.DmgTotal => "SI damage, including incapacitated and post-boom damage; Tank damage is excluded.",
            PlayerResultProperty.HunterDpDmg => "Sum of damage from all recorded high pounces.",
            _ => null
        };
    }

    public static readonly IReadOnlyList<PlayerResultProperty> SummaryColumns =
    [
        PlayerResultProperty.Wins,
        PlayerResultProperty.Loss,
        PlayerResultProperty.Games,
        PlayerResultProperty.MvpSiDamage,
        PlayerResultProperty.MvpCommon,
        PlayerResultProperty.WinRate,
        PlayerResultProperty.RageQuit
    ];

    public static string Format(PlayerResultProperty property, decimal value)
    {
        return property == PlayerResultProperty.WinRate ? value.ToString("P0") : value.ToString("N0");
    }

    public static bool HasPerGame(PlayerResultProperty property)
    {
        return property is not (PlayerResultProperty.Games or PlayerResultProperty.WinRate);
    }

    public static List<PlayerListRow> Ranked(IEnumerable<RankingPlayer> players, bool asc)
    {
        var rows = players.Select(player => new PlayerListRow(player, PlayerResultProperty.Experience));

        return asc
            ? [.. rows.OrderByDescending(row => row.Player.Position).ThenBy(row => row.CommunityId)]
            : [.. rows.OrderBy(row => row.Player.Position).ThenBy(row => row.CommunityId)];
    }

    public static List<PlayerListRow> Sort(IEnumerable<RankingPlayer> players, PlayerResultProperty property, bool asc)
    {
        var rows = players.Select(player => new PlayerListRow(player, property));

        return asc
            ? [.. rows.OrderBy(row => row.Value).ThenBy(row => row.CommunityId)]
            : [.. rows.OrderByDescending(row => row.Value).ThenByDescending(row => row.CommunityId)];
    }
}
