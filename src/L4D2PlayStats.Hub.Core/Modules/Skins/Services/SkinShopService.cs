using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public class SkinShopService(
    HubDbContext dbContext,
    HubState hubState,
    IClock clock,
    ISkinStarsService skinStarsService,
    ISkinService skinService,
    ILogger<SkinShopService> logger) : ISkinShopService
{
    public async Task<SkinWallet> GetWalletAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var stars = await skinStarsService.GetAsync(communityId, cancellationToken);
        var spent = await dbContext.Set<SkinPurchase>().Where(p => p.CommunityId == communityId).SumAsync(p => p.Price, cancellationToken);

        return new SkinWallet(stars, spent);
    }

    public async Task<PlayerSkins> GetPlayerSkinsAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var purchases = await dbContext.Set<SkinPurchase>().AsNoTracking().Where(p => p.CommunityId == communityId).ToListAsync(cancellationToken);
        var equipment = await dbContext.Set<SkinEquipment>().AsNoTracking().Where(e => e.CommunityId == communityId).ToListAsync(cancellationToken);

        return PlayerSkins.Of(communityId, purchases, equipment);
    }

    public async Task<List<PlayerSkins>> GetAllPlayerSkinsAsync(CancellationToken cancellationToken = default)
    {
        var purchases = await dbContext.Set<SkinPurchase>().AsNoTracking().ToListAsync(cancellationToken);
        var equipment = await dbContext.Set<SkinEquipment>().AsNoTracking().ToListAsync(cancellationToken);
        var equipmentByPlayer = equipment.ToLookup(item => item.CommunityId);

        return
        [
            .. purchases
                .GroupBy(purchase => purchase.CommunityId)
                .OrderBy(group => group.Key)
                .Select(group => PlayerSkins.Of(group.Key, group, equipmentByPlayer[group.Key]))
        ];
    }

    public async Task<List<PurchasedSkin>> GetPurchasedSkinsAsync(CancellationToken cancellationToken = default)
    {
        var purchases = dbContext.Set<SkinPurchase>();

        var skins = await dbContext.Set<Skin>()
            .AsNoTracking()
            .Where(skin => purchases.Any(purchase => purchase.SkinId == skin.Id))
            .OrderBy(skin => skin.Id)
            .Select(skin => new { skin.Id, skin.Weapon })
            .ToListAsync(cancellationToken);

        return [.. skins.Select(skin => new PurchasedSkin(skin.Id, skin.Weapon))];
    }

    public Task<List<SkinPurchase>> ListPurchasesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<SkinPurchase>()
            .AsNoTracking()
            .OrderByDescending(p => p.PurchasedAtUtc)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<SkinShopResult> BuyAsync(long communityId, string? playerName, string? skinId, CancellationToken cancellationToken = default)
    {
        return LockedAsync(async () =>
        {
            var skin = await skinService.FindAsync(skinId, cancellationToken);
            if (skin is not { Available: true })
                return SkinShopResult.Of(SkinShopStatus.SkinNotFound);

            if (await OwnsAsync(communityId, skin.Id, cancellationToken))
                return SkinShopResult.Of(SkinShopStatus.AlreadyOwned, skin);

            if (!SteamIds.IsValid(communityId))
                return SkinShopResult.Of(SkinShopStatus.StarsUnavailable, skin);

            var wallet = await GetWalletAsync(communityId, cancellationToken);
            if (wallet.Balance < skin.Price)
                return SkinShopResult.Of(SkinShopStatus.InsufficientStars, skin, wallet);

            var now = clock.UtcNow;

            var purchase = new SkinPurchase
            {
                CommunityId = communityId,
                SkinId = skin.Id,
                Price = skin.Price,
                StarsBefore = wallet.Balance,
                PlayerName = Name(playerName),
                PurchasedAtUtc = now
            };

            dbContext.Add(purchase);
            await EquipAsync(communityId, skin, AppConsts.SkinSourceShop, now, cancellationToken);

            if (!await TrySaveAsync(cancellationToken))
                return SkinShopResult.Of(SkinShopStatus.AlreadyOwned, skin);

            logger.LogInformation("Skin purchased: {CommunityId} bought {SkinId} for {Price} stars (balance {Balance} -> {NewBalance})",
                communityId, skin.Id, skin.Price, wallet.Balance, wallet.Balance - skin.Price);

            return SkinShopResult.Of(SkinShopStatus.Success, skin, wallet.After(skin.Price));
        }, cancellationToken);
    }

    public Task<SkinShopResult> EquipAsync(long communityId, string? skinId, string source, CancellationToken cancellationToken = default)
    {
        return LockedAsync(async () =>
        {
            var skin = await skinService.FindAsync(skinId, cancellationToken);
            if (skin == null)
                return SkinShopResult.Of(SkinShopStatus.SkinNotFound);

            if (!await OwnsAsync(communityId, skin.Id, cancellationToken))
                return SkinShopResult.Of(SkinShopStatus.NotOwned, skin);

            await EquipAsync(communityId, skin, source, clock.UtcNow, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return SkinShopResult.Of(SkinShopStatus.Success, skin);
        }, cancellationToken);
    }

    public Task<SkinShopResult> UnequipAsync(long communityId, string? weapon, CancellationToken cancellationToken = default)
    {
        return LockedAsync(async () =>
        {
            var name = weapon?.Trim();

            if (string.IsNullOrEmpty(name) || !await dbContext.Set<Skin>().AnyAsync(skin => skin.Weapon == name, cancellationToken))
                return SkinShopResult.Of(SkinShopStatus.SkinNotFound);

            var equipment = await dbContext.Set<SkinEquipment>().FirstOrDefaultAsync(e => e.CommunityId == communityId && e.Weapon == name, cancellationToken);
            if (equipment == null)
                return SkinShopResult.Of(SkinShopStatus.Success);

            dbContext.Remove(equipment);
            await dbContext.SaveChangesAsync(cancellationToken);

            return SkinShopResult.Of(SkinShopStatus.Success);
        }, cancellationToken);
    }

    public Task<SkinShopResult> GiftAsync(long communityId, string? skinId, long? giftedBy, string? giftedByName, CancellationToken cancellationToken = default)
    {
        return LockedAsync(async () =>
        {
            var skin = await skinService.FindAsync(skinId, cancellationToken);
            if (skin is not { Available: true })
                return SkinShopResult.Of(SkinShopStatus.SkinNotFound);

            if (await OwnsAsync(communityId, skin.Id, cancellationToken))
                return SkinShopResult.Of(SkinShopStatus.AlreadyOwned, skin);

            var now = clock.UtcNow;

            var purchase = new SkinPurchase
            {
                CommunityId = communityId,
                SkinId = skin.Id,
                Gift = true,
                GiftedByCommunityId = giftedBy,
                GiftedByName = Name(giftedByName),
                PurchasedAtUtc = now
            };

            dbContext.Add(purchase);
            await EquipAsync(communityId, skin, AppConsts.SkinSourceGift, now, cancellationToken);

            if (!await TrySaveAsync(cancellationToken))
                return SkinShopResult.Of(SkinShopStatus.AlreadyOwned, skin);

            logger.LogInformation("Skin gifted: {GiftedBy} gave {SkinId} to {CommunityId}", giftedBy, skin.Id, communityId);

            return SkinShopResult.Of(SkinShopStatus.Success, skin);
        }, cancellationToken);
    }

    public Task<SkinShopResult> RefundAsync(long communityId, string? skinId, long? refundedBy, CancellationToken cancellationToken = default)
    {
        return LockedAsync(async () =>
        {
            var skin = await skinService.FindAsync(skinId, cancellationToken);
            var id = skinId?.Trim();
            var purchases = await dbContext.Set<SkinPurchase>().Where(p => p.CommunityId == communityId && p.SkinId == id).ToListAsync(cancellationToken);

            if (purchases.Count == 0)
                return SkinShopResult.Of(SkinShopStatus.NotOwned, skin);

            await RemoveAsync(purchases, cancellationToken);

            logger.LogInformation("Skin refunded: {RefundedBy} refunded {SkinId} ({Price} stars) to {CommunityId}", refundedBy, id, purchases.Sum(p => p.Price), communityId);

            return SkinShopResult.Of(SkinShopStatus.Success, skin);
        }, cancellationToken);
    }

    public async Task<SkinRefundSummary> RefundAllAsync(long? refundedBy, CancellationToken cancellationToken = default)
    {
        var summary = await RefundAsync(dbContext.Set<SkinPurchase>(), cancellationToken);

        logger.LogWarning("All skin purchases refunded by {RefundedBy}: {Purchases} purchases, {Gifts} gifts, {Stars} stars, {Players} players",
            refundedBy, summary.Purchases, summary.Gifts, summary.Stars, summary.Players);

        return summary;
    }

    public async Task<SkinRefundSummary> RefundPurchasesBeforeAsync(DateTime utcDate, CancellationToken cancellationToken = default)
    {
        var summary = await RefundAsync(dbContext.Set<SkinPurchase>().Where(p => !p.Gift && p.PurchasedAtUtc < utcDate), cancellationToken);

        if (summary.Purchases > 0)
            logger.LogWarning("Skin purchases made before {Date} refunded: {Purchases} purchases, {Stars} stars, {Players} players", utcDate, summary.Purchases, summary.Stars, summary.Players);

        return summary;
    }

    private async Task<SkinRefundSummary> RefundAsync(IQueryable<SkinPurchase> query, CancellationToken cancellationToken)
    {
        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var purchases = await query.ToListAsync(cancellationToken);

            await RemoveAsync(purchases, cancellationToken);

            return SkinRefundSummary.Of(purchases);
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    private async Task RemoveAsync(List<SkinPurchase> purchases, CancellationToken cancellationToken)
    {
        if (purchases.Count == 0)
            return;

        var players = purchases.Select(p => p.CommunityId).Distinct().ToList();
        var refunded = purchases.Select(p => (p.CommunityId, p.SkinId)).ToHashSet();
        var equipment = await dbContext.Set<SkinEquipment>().Where(e => players.Contains(e.CommunityId)).ToListAsync(cancellationToken);

        dbContext.RemoveRange(equipment.Where(e => refunded.Contains((e.CommunityId, e.SkinId))));
        dbContext.RemoveRange(purchases);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EquipAsync(long communityId, Skin skin, string source, DateTime now, CancellationToken cancellationToken)
    {
        var equipment = await dbContext.Set<SkinEquipment>().FirstOrDefaultAsync(e => e.CommunityId == communityId && e.Weapon == skin.Weapon, cancellationToken);

        if (equipment == null)
        {
            equipment = new SkinEquipment { CommunityId = communityId, Weapon = skin.Weapon };
            dbContext.Add(equipment);
        }

        equipment.SkinId = skin.Id;
        equipment.Source = source;
        equipment.UpdatedAtUtc = now;
    }

    private Task<bool> OwnsAsync(long communityId, string skinId, CancellationToken cancellationToken)
    {
        return dbContext.Set<SkinPurchase>().AnyAsync(p => p.CommunityId == communityId && p.SkinId == skinId, cancellationToken);
    }

    private async Task<bool> TrySaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "A skin purchase could not be saved");
            dbContext.ChangeTracker.Clear();

            return false;
        }
    }

    private async Task<SkinShopResult> LockedAsync(Func<Task<SkinShopResult>> action, CancellationToken cancellationToken)
    {
        if (!await hubState.WriteLock.WaitAsync(TimeSpan.FromSeconds(AppConsts.SkinShopLockTimeoutInSeconds), cancellationToken))
            return SkinShopResult.Of(SkinShopStatus.Busy);

        try
        {
            return await action();
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    private static string? Name(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var trimmed = name.Trim();

        return trimmed.Length <= AppConsts.SkinNameMaxLength ? trimmed : trimmed[..AppConsts.SkinNameMaxLength];
    }
}