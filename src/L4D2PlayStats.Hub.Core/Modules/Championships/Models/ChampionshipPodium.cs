namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record ChampionshipPodium(int? Champion, int? RunnerUp, int? Third)
{
    public static readonly ChampionshipPodium None = new(null, null, null);
}
