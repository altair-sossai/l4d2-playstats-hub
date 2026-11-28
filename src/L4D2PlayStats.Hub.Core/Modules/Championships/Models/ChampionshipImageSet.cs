namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record ChampionshipImageSet(string? Banner, string? Logo, IReadOnlyList<string> Gallery)
{
    public IEnumerable<string> Files => new[] { Banner, Logo }.OfType<string>().Concat(Gallery);
}
