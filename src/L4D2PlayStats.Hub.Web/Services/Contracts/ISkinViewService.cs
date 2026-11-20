using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Web.Models.Skins;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface ISkinViewService
{
    Task<SkinsModel> ShopAsync(CancellationToken cancellationToken = default);
    Task<SkinPurchasesAdminModel> PurchasesAsync(string? query, string? player, int page, CancellationToken cancellationToken = default);
    Task<SkinPricesAdminModel> PricesAsync(string? query, SkinRarity? rarity, string? weapon, int page, CancellationToken cancellationToken = default);
    Task<SkinCatalogViewModel> CatalogAsync(CancellationToken cancellationToken = default);
}
