using L4D2PlayStats.Hub.Core.Modules.Campaigns.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Campaigns.Services.Contracts;

public interface ICampaignService
{
    Task<CampaignCatalog> GetCatalogAsync(CancellationToken cancellationToken = default);
    Task<List<Campaign>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<Campaign>> ListAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task SeedAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(int? id, string name, IReadOnlyList<string> maps, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
