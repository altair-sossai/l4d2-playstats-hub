using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipRandomService : IChampionshipRandomService
{
    public int Next(int exclusiveMaximum)
    {
        return exclusiveMaximum <= 1 ? 0 : Random.Shared.Next(exclusiveMaximum);
    }
}
