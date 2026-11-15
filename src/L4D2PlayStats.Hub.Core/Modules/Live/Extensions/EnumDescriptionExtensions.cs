using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Extensions;

public static class EnumDescriptionExtensions
{
    private static readonly ConcurrentDictionary<Enum, string> Descriptions = new();

    public static string Description(this Enum value)
    {
        return Descriptions.GetOrAdd(value, static key =>
        {
            var name = key.ToString();
            var field = key.GetType().GetField(name);

            return field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;
        });
    }
}
