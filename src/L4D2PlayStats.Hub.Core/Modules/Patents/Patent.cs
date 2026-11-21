namespace L4D2PlayStats.Hub.Core.Modules.Patents;

public class Patent(int level, string name, int experience)
{
    public int Level { get; } = level;
    public string Name { get; } = name;
    public int Experience { get; } = experience;
    public string FullName => $"Level {Level:00} - {Name}";
    public string Image => $"/imgs/patents/{Level:00}.png";
}
