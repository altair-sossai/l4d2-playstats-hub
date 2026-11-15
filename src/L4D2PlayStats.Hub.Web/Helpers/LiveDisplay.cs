using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class LiveDisplay
{
    public static string? PlayerUrl(LivePlayer? player, bool ranking)
    {
        if (player?.Id == null)
            return null;

        return ranking ? $"/player/{player.Id}" : player.ProfileUrl;
    }

    public static string Avatar(LivePlayer player)
    {
        return player.Id == null ? AppConsts.EmptyAvatarUrl : $"/avatar/{player.Id}";
    }

    public static string Width(decimal fraction)
    {
        var percentage = Math.Round(Math.Clamp(fraction, 0, 1) * 100, 2);

        return percentage.ToString(CultureInfo.InvariantCulture) + "%";
    }

    public static decimal Fraction(int value, int total)
    {
        return total <= 0 ? 0 : value / (decimal)total;
    }

    public static string Stars(int stars)
    {
        return new string('★', Math.Max(0, stars));
    }

    public static string Time(DateTime utc, TimeZoneInfo timeZone)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), timeZone);

        return local.ToString(AppConsts.LiveTimeFormat, CultureInfo.InvariantCulture);
    }

    public static IReadOnlyList<string> Weapons(LiveSurvivor survivor)
    {
        Weapon?[] slots = [survivor.PrimaryWeapon, survivor.SecondaryWeapon, survivor.SlotNumber3, survivor.SlotNumber4, survivor.SlotNumber5];

        return
        [
            .. slots
                .Where(weapon => weapon != null && weapon != Weapon.None && Enum.IsDefined(weapon.Value))
                .Select(weapon => weapon == Weapon.Melee && survivor.MeleeWeapon is { } melee && melee != MeleeWeapon.MeleeNone && Enum.IsDefined(melee)
                    ? melee.Description()
                    : weapon!.Value.Description())
        ];
    }

    public static string HealthClass(HealthColor? color)
    {
        return color switch
        {
            HealthColor.Green => "bg-success",
            HealthColor.Orange => "bg-warning",
            HealthColor.Red => "bg-danger",
            _ => "bg-secondary"
        };
    }

    public static string LatencyClass(LatencyType latency)
    {
        return latency switch
        {
            LatencyType.Low => "text-bg-success",
            LatencyType.Medium => "text-bg-warning",
            _ => "text-bg-danger"
        };
    }
}
