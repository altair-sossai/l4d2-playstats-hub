using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.SkinShop)]
[Route("admin/skins")]
public class SkinsAdminController(
    ISkinViewService skinViewService,
    ISkinCatalogService skinCatalogService,
    ISkinAuthorSyncService skinAuthorSyncService,
    ISettingsService settingsService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Purchases(string? q, string? player, int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.Admin = "active";
        ViewBag.SkinPurchases = "active";

        return View(await skinViewService.PurchasesAsync(q, player, page, cancellationToken));
    }

    [HttpGet("purchases")]
    public IActionResult LegacyPurchases()
    {
        return RedirectPermanent($"{AppConsts.SkinAdminUrl}{Request.QueryString}");
    }

    [HttpGet("prices")]
    public async Task<IActionResult> Prices(string? q, SkinRarity? rarity, string? weapon, int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.Admin = "active";
        ViewBag.SkinPrices = "active";

        return View(await skinViewService.PricesAsync(q, rarity, weapon, page, cancellationToken));
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog(CancellationToken cancellationToken)
    {
        ViewBag.Admin = "active";

        return View(await skinViewService.CatalogAsync(cancellationToken));
    }

    [HttpPost("catalog/upload")]
    [RequestSizeLimit(CoreConsts.SkinCatalogMaxBytes)]
    public async Task<IActionResult> UploadCatalog(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = AppConsts.SkinCatalogNoFile;
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

    [HttpPost("catalog/bundled")]
    public async Task<IActionResult> ImportBundledCatalog(CancellationToken cancellationToken)
    {
        return Imported(await skinCatalogService.ImportBundledAsync(cancellationToken));
    }

    [HttpPost("catalog/authors")]
    public async Task<IActionResult> SyncAuthors(CancellationToken cancellationToken)
    {
        var result = await skinAuthorSyncService.SyncAsync(true, cancellationToken);

        if (result.Due > 0 && result.Checked == 0)
            TempData["Error"] = AppConsts.SkinAuthorSyncNoAnswer;
        else
            TempData[AppConsts.SkinAuthorSyncTempData] = string.Join(AppConsts.SkinTempDataSeparator, result.Checked, result.Renamed);

        return RedirectToAction(nameof(Catalog));
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        ViewBag.Admin = "active";

        return View(await settingsService.GetAsync<SkinShopSettings>(cancellationToken));
    }

    [HttpPost("settings")]
    public async Task<IActionResult> SaveSettings(SkinShopSettings settings, CancellationToken cancellationToken)
    {
        settings.AssetsUrl = Clean(settings.AssetsUrl)?.TrimEnd('/');
        settings.CatalogUrl = Clean(settings.CatalogUrl);
        settings.AuthorContactUrl = Clean(settings.AuthorContactUrl);
        settings.AuthorRequestUrl = Clean(settings.AuthorRequestUrl);

        string?[] addresses = [settings.AssetsUrl, settings.CatalogUrl, settings.AuthorContactUrl, settings.AuthorRequestUrl];

        if (addresses.All(address => address == null || SkinAssets.IsHttpUrl(address)))
        {
            await settingsService.SaveAsync(settings, cancellationToken);
            TempData["Saved"] = true;
        }
        else
            TempData["Error"] = AppConsts.SkinSettingsInvalidUrl;

        return RedirectToAction(nameof(Settings));
    }

    private IActionResult Imported(SkinCatalogImportResult result)
    {
        if (result.Succeeded)
            TempData[AppConsts.SkinImportTempData] = string.Join(AppConsts.SkinTempDataSeparator, result.Total, result.Added, result.Updated, result.Removed);
        else
            TempData["Error"] = SkinDisplay.CatalogMessage(result.Status);

        return RedirectToAction(nameof(Catalog));
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
