using System.Collections.Concurrent;
using System.Reflection;
using L4D2PlayStats.Hub.Core.Modules.Live.Attributes;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Extensions;

public static class WeaponNameExtensions
{
    private static readonly ConcurrentDictionary<Enum, string?> Names = new();

    public static string? WeaponName(this Weapon weapon)
    {
        return Find(weapon);
    }

    public static string? WeaponName(this MeleeWeapon meleeWeapon)
    {
        return Find(meleeWeapon);
    }

    private static string? Find(Enum value)
    {
        return Names.GetOrAdd(value, static key =>
        {
            var field = key.GetType().GetField(key.ToString());

            return field?.GetCustomAttribute<WeaponNameAttribute>()?.Name;
        });
    }
}