using System.Linq.Expressions;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinWeapons
{
    private static readonly (string Weapon, string Name)[] Known =
    [
        ("weapon_smg", "SMG"),
        ("weapon_smg_silenced", "MAC-10"),
        ("weapon_smg_mp5", "MP5"),
        ("weapon_pumpshotgun", "Pump Shotgun"),
        ("weapon_shotgun_chrome", "Chrome Shotgun"),
        ("weapon_pistol_magnum", "Magnum"),
        ("weapon_melee_fireaxe", "Fireaxe"),
        ("weapon_melee_katana", "Katana"),
        ("weapon_melee_machete", "Machete"),
        ("weapon_melee_baseball_bat", "Baseball Bat"),
        ("weapon_melee_cricket_bat", "Cricket Bat"),
        ("weapon_melee_electric_guitar", "Electric Guitar"),
        ("weapon_melee_crowbar", "Crowbar"),
        ("weapon_melee_golfclub", "Golf Club"),
        ("weapon_melee_frying_pan", "Frying Pan")
    ];

    public static IReadOnlyList<string> All { get; } = [.. Known.Select(item => item.Weapon)];

    public static string Name(string weapon)
    {
        var known = Known.FirstOrDefault(item => item.Weapon == weapon);

        return known.Name ?? weapon;
    }

    public static int Order(string weapon)
    {
        var index = Array.FindIndex(Known, item => item.Weapon == weapon);

        return index < 0 ? Known.Length : index;
    }

    public static Expression<Func<Skin, int>> OrderOf()
    {
        var skin = Expression.Parameter(typeof(Skin));
        var weapon = Expression.Property(skin, nameof(Skin.Weapon));
        Expression order = Expression.Constant(Known.Length);

        for (var index = Known.Length - 1; index >= 0; index--)
            order = Expression.Condition(Expression.Equal(weapon, Expression.Constant(Known[index].Weapon)), Expression.Constant(index), order);

        return Expression.Lambda<Func<Skin, int>>(order, skin);
    }

    public static bool IsMelee(string weapon)
    {
        return weapon.StartsWith(AppConsts.SkinMeleePrefix, StringComparison.Ordinal);
    }

    public static List<string> Sort(IEnumerable<string> weapons)
    {
        return [.. weapons.Distinct().OrderBy(Order).ThenBy(weapon => weapon)];
    }
}
