using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Skins;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Skins;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Services;

public class SkinViewService(
    HubDbContext dbContext,
    ISkinService skinService,
    ISkinShopService skinShopService,
    ISeasonService seasonService,
    ISettingsService settingsService,
    ISkinAuthorService skinAuthorService,
    ICurrentUser currentUser,
    SkinThumbnailStore skinThumbnailStore) : ISkinViewService
{
    public async Task<SkinsModel> ShopAsync(CancellationToken cancellationToken = default)
    {
        var skins = await skinService.ListAsync(cancellationToken);
        var settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken);
        var season = await SeasonAsync(settings, cancellationToken);
        var communityId = currentUser.CommunityId;

        SkinWallet? wallet = null;
        PlayerSkins? playerSkins = null;

        if (communityId != null)
        {
            wallet = await skinShopService.GetWalletAsync(communityId.Value, cancellationToken);
            playerSkins = await skinShopService.GetPlayerSkinsAsync(communityId.Value, cancellationToken);
        }

        return new SkinsModel(skins, currentUser.IsAuthenticated, wallet, playerSkins, settings, season, skinThumbnailStore);
    }

    public async Task<SkinPurchasesAdminModel> PurchasesAsync(string? query, string? player, int page, CancellationToken cancellationToken = default)
    {
        var culture = CultureInfo.CurrentUICulture;
        var skins = await skinService.ListAsync(cancellationToken);
        var skinsById = skins.ToDictionary(skin => skin.Id);
        var purchases = await skinShopService.ListPurchasesAsync(cancellationToken);
        var settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken);
        var timeZone = await TimeZoneAsync(cancellationToken);
        var playerId = long.TryParse(player?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ? parsed : (long?)null;

        var ids = purchases.Select(purchase => purchase.CommunityId)
            .Concat(purchases.Where(purchase => purchase.GiftedByCommunityId != null).Select(purchase => purchase.GiftedByCommunityId!.Value))
            .Append(playerId ?? 0)
            .Distinct()
            .ToList();

        var names = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(profile => profile.Name != null && ids.Contains(profile.CommunityId))
            .ToDictionaryAsync(profile => profile.CommunityId, profile => profile.Name!, cancellationToken);

        var rows = purchases
            .Where(purchase => playerId == null || purchase.CommunityId == playerId)
            .Select(purchase => new SkinPurchaseRowModel(
                purchase,
                skinsById.GetValueOrDefault(purchase.SkinId),
                purchase.PlayerName ?? names.GetValueOrDefault(purchase.CommunityId) ?? Text(purchase.CommunityId),
                purchase.GiftedByCommunityId == null ? null : purchase.GiftedByName ?? names.GetValueOrDefault(purchase.GiftedByCommunityId.Value) ?? Text(purchase.GiftedByCommunityId.Value),
                ToSiteTime(purchase.PurchasedAtUtc, timeZone)))
            .Where(row => row.Matches(query, culture))
            .ToList();

        var playerName = playerId == null
            ? null
            : purchases.FirstOrDefault(purchase => purchase.CommunityId == playerId && purchase.PlayerName != null)?.PlayerName
              ?? names.GetValueOrDefault(playerId.Value)
              ?? Text(playerId.Value);

        return new SkinPurchasesAdminModel
        {
            Purchases = new PagedListModel<SkinPurchaseRowModel>(rows, page, AppConsts.SkinListPageSize),
            Query = query,
            Player = playerId == null ? null : Text(playerId.Value),
            PlayerName = playerName,
            Skins = [.. skins.Where(skin => skin.Available)],
            AssetsUrl = settings.AssetsUrl,
            Season = await SeasonAsync(settings, cancellationToken),
            TotalPurchases = purchases.Count(purchase => !purchase.Gift),
            TotalStars = purchases.Sum(purchase => purchase.Price),
            TotalGifts = purchases.Count(purchase => purchase.Gift)
        };
    }

    public async Task<SkinPricesAdminModel> PricesAsync(string? query, SkinRarity? rarity, string? weapon, int page, CancellationToken cancellationToken = default)
    {
        var culture = CultureInfo.CurrentUICulture;
        var skins = await skinService.ListAsync(cancellationToken);
        var settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken);
        var term = query?.Trim();

        var filtered = skins
            .Where(skin => rarity == null || skin.Rarity == rarity)
            .Where(skin => string.IsNullOrEmpty(weapon) || skin.Weapon == weapon)
            .Where(skin => string.IsNullOrEmpty(term)
                           || skin.Id.Contains(term, StringComparison.OrdinalIgnoreCase)
                           || skin.Title(culture).Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(skin => skin.Rarity)
            .ThenByDescending(skin => skin.Price)
            .ThenBy(skin => SkinWeapons.Order(skin.Weapon))
            .ThenBy(skin => skin.Id)
            .ToList();

        return new SkinPricesAdminModel(
            new PagedListModel<Skin>(filtered, page, AppConsts.SkinListPageSize),
            [.. skins.Where(skin => skin.Available)],
            SkinWeapons.Sort(SkinWeapons.All.Concat(skins.Select(skin => skin.Weapon))),
            settings.AssetsUrl,
            query,
            rarity,
            weapon,
            skins.Count(skin => skin.HasCustomPrice));
    }

    public async Task<SkinCatalogViewModel> CatalogAsync(CancellationToken cancellationToken = default)
    {
        var skins = dbContext.Set<Skin>().AsNoTracking();
        var total = await skins.CountAsync(cancellationToken);
        var available = await skins.CountAsync(skin => skin.Available, cancellationToken);
        var timeZone = await TimeZoneAsync(cancellationToken);
        DateTime? updatedAtUtc = total == 0 ? null : await skins.MaxAsync(skin => skin.UpdatedAtUtc, cancellationToken);
        var authors = await skinAuthorService.StatusAsync(cancellationToken);

        return new SkinCatalogViewModel
        {
            Settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken),
            Total = total,
            Available = available,
            Unavailable = total - available,
            Bundled = SkinBundledCatalog.Count,
            UpdatedAt = updatedAtUtc == null ? null : ToSiteTime(updatedAtUtc.Value, timeZone),
            Authors = authors,
            AuthorsSyncedAt = authors.LastSyncedAtUtc == null ? null : ToSiteTime(authors.LastSyncedAtUtc.Value, timeZone),
            AuthorRenamedAt = authors.LastRenamedAtUtc == null ? null : ToSiteTime(authors.LastRenamedAtUtc.Value, timeZone)
        };
    }

    private async Task<SkinSeasonModel> SeasonAsync(SkinShopSettings settings, CancellationToken cancellationToken)
    {
        var season = await seasonService.GetCurrentAsync(cancellationToken);
        var now = await seasonService.SiteNowAsync(cancellationToken);

        return new SkinSeasonModel
        {
            LastDay = season.EndsAt.AddDays(-1),
            DaysLeft = Math.Max(0, (int)Math.Floor((season.EndsAt - now).TotalDays)),
            LengthInMonths = (season.EndsAt.Year - season.StartsAt.Year) * AppConsts.SkinSeasonMonthsInYear + season.EndsAt.Month - season.StartsAt.Month,
            Refunds = settings.RefundAtSeasonEnd
        };
    }

    private async Task<TimeZoneInfo> TimeZoneAsync(CancellationToken cancellationToken)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        return SiteTime.Zone(site.TimeZone);
    }

    private static DateTime ToSiteTime(DateTime utc, TimeZoneInfo timeZone)
    {
        return SiteTime.FromUtc(utc, timeZone);
    }

    private static string Text(long communityId)
    {
        return communityId.ToString(CultureInfo.InvariantCulture);
    }
}
