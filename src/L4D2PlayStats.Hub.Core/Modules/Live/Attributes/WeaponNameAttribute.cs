namespace L4D2PlayStats.Hub.Core.Modules.Live.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class WeaponNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}