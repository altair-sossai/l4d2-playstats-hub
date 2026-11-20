using L4D2PlayStats.Hub.Core.Modules.Skins.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;

public interface ISkinShopService
{
    Task<SkinWallet> GetWalletAsync(long communityId, CancellationToken cancellationToken = default);
    Task<PlayerSkins> GetPlayerSkinsAsync(long communityId, CancellationToken cancellationToken = default);
    Task<List<PlayerSkins>> GetAllPlayerSkinsAsync(CancellationToken cancellationToken = default);
    Task<List<PurchasedSkin>> GetPurchasedSkinsAsync(CancellationToken cancellationToken = default);
    Task<List<SkinPurchase>> ListPurchasesAsync(CancellationToken cancellationToken = default);
    Task<SkinShopResult> BuyAsync(long communityId, string? playerName, string? skinId, CancellationToken cancellationToken = default);
    Task<SkinShopResult> EquipAsync(long communityId, string? skinId, string source, CancellationToken cancellationToken = default);
    Task<SkinShopResult> UnequipAsync(long communityId, string? weapon, CancellationToken cancellationToken = default);
    Task<SkinShopResult> GiftAsync(long communityId, string? skinId, long? giftedBy, string? giftedByName, CancellationToken cancellationToken = default);
    Task<SkinShopResult> RefundAsync(long communityId, string? skinId, long? refundedBy, CancellationToken cancellationToken = default);
    Task<SkinRefundSummary> RefundAllAsync(long? refundedBy, CancellationToken cancellationToken = default);
    Task<SkinRefundSummary> RefundPurchasesBeforeAsync(DateTime utcDate, CancellationToken cancellationToken = default);
}
