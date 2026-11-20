using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.Settings.Attributes;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

public static class SecretProperties
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<string>> Names = new();
    private static readonly Lazy<IReadOnlyList<Type>> Types = new(FindTypes);

    public static IReadOnlyList<string> Of(Type type)
    {
        return Names.GetOrAdd(type, Find);
    }

    public static IReadOnlyList<Type> SettingTypes()
    {
        return Types.Value;
    }

    private static IReadOnlyList<string> Find(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType == typeof(string) && property.IsDefined(typeof(SecretAttribute), true))
            .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
            .ToList();
    }

    private static IReadOnlyList<Type> FindTypes()
    {
        return typeof(SecretAttribute).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && Of(type).Count > 0)
            .ToList();
    }
}
