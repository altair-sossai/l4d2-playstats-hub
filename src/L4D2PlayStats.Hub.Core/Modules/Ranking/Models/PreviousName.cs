namespace L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

public class PreviousName(string name)
{
    public string Name { get; } = name;
    public int TimesUsed { get; set; }
}
