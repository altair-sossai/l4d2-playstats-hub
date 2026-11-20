namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public class MatchDraft
{
    public string? Campaign { get; init; }
    public string ConfigurationName { get; init; } = null!;
    public int TeamSize { get; init; }
    public List<AssemblyFile> Files { get; } = [];

    public AssemblyFile Newest => Files[0];
    public AssemblyFile Oldest => Files[^1];
}