namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipRandomService
{
    int Next(int exclusiveMaximum);
}
