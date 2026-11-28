using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Web.Models.Championships;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IChampionshipViewService
{
    Task<ChampionshipPageModel> PageAsync(Championship championship, string tab, ChampionshipSeries? series = null, CancellationToken cancellationToken = default);
}
