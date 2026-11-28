namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IChampionshipFlagService
{
    Task ApplyAsync(int championshipId, IFormFile? upload, Func<string?, Task<List<string>>> apply, CancellationToken cancellationToken = default);
    void Delete(int championshipId, IEnumerable<string> files);
}
