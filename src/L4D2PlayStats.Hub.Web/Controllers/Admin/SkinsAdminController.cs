using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Skins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.SkinShop)]
[Route("admin/skins")]
public class SkinsAdminController(
    HubDbContext dbContext,
    ISkinService skinService,
    ISkinShopService skinShopService,
    ISkinCatalogService skinCatalogService,
    ISettingsService settingsService,
    ICurrentUser currentUser) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Purchases(string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var culture = CultureInfo.CurrentUICulture;
        var skins = await skinService.ListAsync(cancellationToken);
        var skinsById = skins.ToDictionary(skin => skin.Id);
        var purchases = await skinShopService.ListPurchasesAsync(cancellationToken);
        var names = await dbContext.PlayerProfiles.AsNoTracking().Where(profile => profile.Name != null).ToDictionaryAsync(profile => profile.CommunityId, profile => profile.Name!, cancellationToken);

        var rows = purchases
            .Select(purchase => new SkinPurchaseRow(
                purchase,
                skinsById.GetValueOrDefault(purchase.SkinId),
                purchase.PlayerName ?? names.GetValueOrDefault(purchase.CommunityId) ?? purchase.CommunityId.ToString(),
                purchase.GiftedByCommunityId == null ? null : purchase.GiftedByName ?? names.GetValueOrDefault(purchase.GiftedByCommunityId.Value) ?? purchase.GiftedByCommunityId.ToString()))
            .Where(row => row.Matches(q, culture))
            .ToList();

        var model = new SkinPurchasesViewModel
        {
            Rows = new PagedList<SkinPurchaseRow>(rows, page, AppConsts.SkinAdminPageSize),
            Skins = [.. skins.Where(skin => skin.Available).OrderBy(skin => SkinWeapons.Order(skin.Weapon)).ThenBy(skin => skin.Title(culture))],
            Query = q,
            Purchases = purchases.Count(purchase => !purchase.Gift),
            Gifts = purchases.Count(purchase => purchase.Gift),
            Stars = purchases.Sum(purchase => purchase.Price),
            Players = purchases.Select(purchase => purchase.CommunityId).Distinct().Count()
        };

        return View(model);
    }

    [HttpPost("gift")]
    public async Task<IActionResult> Gift(long communityId, string? skinId, CancellationToken cancellationToken)
    {
        if (!SteamIds.IsValid(communityId))
        {
            TempData["Error"] = "That is not a valid SteamID64.";
            return RedirectToAction(nameof(Purchases));
        }

        var result = await skinShopService.GiftAsync(communityId, skinId, currentUser.CommunityId, currentUser.Name, cancellationToken);

        if (result.Succeeded)
            TempData[AppConsts.SkinMessageTempData] = "Skin given to the player.";
        else
            TempData["Error"] = SkinShopMessages.For(result.Status);

        return RedirectToAction(nameof(Purchases));
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund(long communityId, string? skinId, CancellationToken cancellationToken)
    {
        var result = await skinShopService.RefundAsync(communityId, skinId, currentUser.CommunityId, cancellationToken);

        if (result.Succeeded)
            TempData[AppConsts.SkinMessageTempData] = "Skin refunded.";
        else
            TempData["Error"] = SkinShopMessages.For(result.Status);

        return RedirectToAction(nameof(Purchases));
    }

    [HttpPost("refund-all")]
    public async Task<IActionResult> RefundAll(CancellationToken cancellationToken)
    {
        var summary = await skinShopService.RefundAllAsync(currentUser.CommunityId, cancellationToken);

        TempData[AppConsts.SkinRefundTempData] = string.Join(AppConsts.SkinTempDataSeparator, summary.Purchases, summary.Gifts, summary.Stars, summary.Players);

        return RedirectToAction(nameof(Purchases));
    }

    [HttpGet("prices")]
    public async Task<IActionResult> Prices(string? q, string? weapon, SkinRarity? rarity, int page = 1, CancellationToken cancellationToken = default)
    {
        var culture = CultureInfo.CurrentUICulture;
        var skins = await skinService.ListAsync(cancellationToken);
        var term = q?.Trim();

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

        var list = new PagedList<Skin>(filtered, page, AppConsts.SkinAdminPageSize);

        var model = new SkinPricesViewModel
        {
            Skins = list,
            Weapons = SkinWeapons.Sort(skins.Select(skin => skin.Weapon)),
            Query = q,
            Weapon = weapon,
            Rarity = rarity,
            CustomPrices = skins.Count(skin => skin.HasCustomPrice),
            ReturnUrl = AppConsts.SkinPricesUrl + SkinDisplay.Query(("q", q), ("weapon", weapon), ("rarity", rarity), ("page", list.Page))
        };

        return View(model);
    }

    [HttpPost("prices")]
    public async Task<IActionResult> SavePrice(string? skinId, int? price, string? returnUrl, CancellationToken cancellationToken)
    {
        if (await skinService.SetPriceAsync(skinId, price, currentUser.CommunityId, cancellationToken))
            TempData[AppConsts.SkinMessageTempData] = "Price saved.";
        else
            TempData["Error"] = "That price is not valid for that skin.";

        var local = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.StartsWith(AppConsts.SkinPricesUrl, StringComparison.Ordinal);

        return Redirect(local ? returnUrl! : AppConsts.SkinPricesUrl);
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog(CancellationToken cancellationToken)
    {
        var skins = dbContext.Set<Skin>().AsNoTracking();
        var total = await skins.CountAsync(cancellationToken);
        var available = await skins.CountAsync(skin => skin.Available, cancellationToken);

        var model = new SkinCatalogViewModel
        {
            Settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken),
            Total = total,
            Available = available,
            Unavailable = total - available,
            UpdatedAtUtc = total == 0 ? null : await skins.MaxAsync(skin => skin.UpdatedAtUtc, cancellationToken)
        };

        return View(model);
    }

    [HttpPost("catalog/upload")]
    [RequestSizeLimit(CoreConsts.SkinCatalogMaxBytes)]
    public async Task<IActionResult> UploadCatalog(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Choose the catalog file first.";
            return RedirectToAction(nameof(Catalog));
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var json = await reader.ReadToEndAsync(cancellationToken);

        return Imported(await skinCatalogService.ImportAsync(json, cancellationToken));
    }

    [HttpPost("catalog/fetch")]
    public async Task<IActionResult> FetchCatalog(CancellationToken cancellationToken)
    {
        return Imported(await skinCatalogService.ImportFromUrlAsync(cancellationToken));
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        return View(await settingsService.GetAsync<SkinShopSettings>(cancellationToken));
    }

    [HttpPost("settings")]
    public async Task<IActionResult> SaveSettings(SkinShopSettings settings, CancellationToken cancellationToken)
    {
        settings.AssetsUrl = string.IsNullOrWhiteSpace(settings.AssetsUrl) ? null : settings.AssetsUrl.Trim().TrimEnd('/');
        settings.CatalogUrl = string.IsNullOrWhiteSpace(settings.CatalogUrl) ? null : settings.CatalogUrl.Trim();

        var valid = (settings.AssetsUrl == null || SkinAssets.IsHttpUrl(settings.AssetsUrl)) && (settings.CatalogUrl == null || SkinAssets.IsHttpUrl(settings.CatalogUrl));

        if (valid)
        {
            await settingsService.SaveAsync(settings, cancellationToken);
            TempData["Saved"] = true;
        }
        else
        {
            TempData["Error"] = "An address must start with http:// or https://.";
        }

        return RedirectToAction(nameof(Settings));
    }

    private IActionResult Imported(SkinCatalogImportResult result)
    {
        if (result.Succeeded)
            TempData[AppConsts.SkinImportTempData] = string.Join(AppConsts.SkinTempDataSeparator, result.Total, result.Added, result.Updated, result.Removed);
        else
            TempData["Error"] = SkinShopMessages.For(result.Status);

        return RedirectToAction(nameof(Catalog));
    }
}
