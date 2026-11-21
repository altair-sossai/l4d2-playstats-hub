using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
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
    SkinThumbnailStore skinThumbnailStore,
    IPlayerNameService playerNameService,
    ISiteClock siteClock) : ISkinViewService
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
        var settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);
        var playerId = SteamIdParser.Parse(player);
        var search = PlayerSearch.Of(query);
        List<string> titled = search.IsEmpty ? [] : [.. skins.Where(skin => skin.Title(culture).Contains(search.Term!, StringComparison.OrdinalIgnoreCase)).Select(skin => skin.Id)];
        var purchases = await skinShopService.PurchasesAsync(playerId, search, titled, page, AppConsts.SkinListPageSize, cancellationToken);

        List<long> ids =
        [
            .. purchases.Items.Select(purchase => purchase.CommunityId),
            .. purchases.Items.Where(purchase => purchase.GiftedByCommunityId != null).Select(purchase => purchase.GiftedByCommunityId!.Value),
            .. playerId == null ? [] : new[] { playerId.Value }
        ];

        var names = await playerNameService.NamesAsync(ids, cancellationToken);
        var totals = await skinShopService.PurchaseTotalsAsync(cancellationToken);

        var rows = purchases.Map(purchase => new SkinPurchaseRowModel(
            purchase,
            skinsById.GetValueOrDefault(purchase.SkinId),
            purchase.PlayerName ?? names[purchase.CommunityId],
            purchase.GiftedByCommunityId == null ? null : purchase.GiftedByName ?? names[purchase.GiftedByCommunityId.Value],
            SiteTime.FromUtc(purchase.PurchasedAtUtc, timeZone)));

        return new SkinPurchasesAdminModel
        {
            Purchases = new PagedListModel<SkinPurchaseRowModel>(rows),
            Query = query,
            Player = playerId?.ToString(CultureInfo.InvariantCulture),
            PlayerName = playerId == null ? null : await skinShopService.PurchaserNameAsync(playerId.Value, cancellationToken) ?? names[playerId.Value],
            Skins = [.. skins.Where(skin => skin.Available)],
            AssetsUrl = settings.AssetsUrl,
            Season = await SeasonAsync(settings, cancellationToken),
            TotalPurchases = totals.Purchases,
            TotalStars = totals.Stars,
            TotalGifts = totals.Gifts
        };
    }

    public async Task<SkinPricesAdminModel> PricesAsync(string? query, SkinRarity? rarity, string? weapon, int page, CancellationToken cancellationToken = default)
    {
        var culture = CultureInfo.CurrentUICulture;
        var skins = await skinService.ListAsync(cancellationToken);
        var settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken);
        var term = query?.Trim();

        List<string>? ids = string.IsNullOrEmpty(term)
            ? null
            : [.. skins.Where(skin => skin.Id.Contains(term, StringComparison.OrdinalIgnoreCase) || skin.Title(culture).Contains(term, StringComparison.OrdinalIgnoreCase)).Select(skin => skin.Id)];

        var listed = await skinService.PageAsync(rarity, weapon, ids, page, AppConsts.SkinListPageSize, cancellationToken);

        return new SkinPricesAdminModel(
            new PagedListModel<Skin>(listed),
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
        var timeZone = await siteClock.ZoneAsync(cancellationToken);
        DateTime? updatedAtUtc = total == 0 ? null : await skins.MaxAsync(skin => skin.UpdatedAtUtc, cancellationToken);
        var authors = await skinAuthorService.StatusAsync(cancellationToken);

        return new SkinCatalogViewModel
        {
            Settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken),
            Total = total,
            Available = available,
            Unavailable = total - available,
            Bundled = SkinBundledCatalog.Count,
            UpdatedAt = updatedAtUtc == null ? null : SiteTime.FromUtc(updatedAtUtc.Value, timeZone),
            Authors = authors,
            AuthorsSyncedAt = authors.LastSyncedAtUtc == null ? null : SiteTime.FromUtc(authors.LastSyncedAtUtc.Value, timeZone),
            AuthorRenamedAt = authors.LastRenamedAtUtc == null ? null : SiteTime.FromUtc(authors.LastRenamedAtUtc.Value, timeZone)
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
}
