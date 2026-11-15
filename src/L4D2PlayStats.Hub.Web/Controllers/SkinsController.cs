using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Models.Skins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.SkinShop)]
[AutoValidateAntiforgeryToken]
public class SkinsController(
    ISkinService skinService,
    ISkinShopService skinShopService,
    ISettingsService settingsService,
    ICurrentUser currentUser) : Controller
{
    [HttpGet("/skins")]
    public async Task<IActionResult> Index(string? weapon, SkinRarity? rarity, bool owned = false, CancellationToken cancellationToken = default)
    {
        var culture = CultureInfo.CurrentUICulture;
        var communityId = currentUser.CommunityId;
        var skins = await skinService.ListAsync(cancellationToken);
        var playerSkins = communityId == null ? new PlayerSkins() : await skinShopService.GetPlayerSkinsAsync(communityId.Value, cancellationToken);
        var visible = skins.Where(skin => skin.Available || playerSkins.Owns(skin.Id)).ToList();
        var onlyOwned = owned && communityId != null;

        var filtered = visible
            .Where(skin => string.IsNullOrEmpty(weapon) || skin.Weapon == weapon)
            .Where(skin => rarity == null || skin.Rarity == rarity)
            .Where(skin => !onlyOwned || playerSkins.Owns(skin.Id))
            .OrderByDescending(skin => skin.Rarity)
            .ThenByDescending(skin => skin.Price)
            .ThenBy(skin => SkinWeapons.Order(skin.Weapon))
            .ThenBy(skin => skin.Title(culture))
            .ToList();

        var model = new SkinShopViewModel
        {
            Skins = filtered,
            Weapons = SkinWeapons.Sort(visible.Select(skin => skin.Weapon)),
            Weapon = weapon,
            Rarity = rarity,
            OnlyOwned = onlyOwned,
            SignedIn = communityId != null,
            Wallet = communityId == null ? null : await skinShopService.GetWalletAsync(communityId.Value, cancellationToken),
            PlayerSkins = playerSkins,
            Settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken),
            Total = visible.Count,
            ReturnUrl = AppConsts.SkinShopUrl + SkinDisplay.Query(("weapon", weapon), ("rarity", rarity), ("owned", onlyOwned ? "true" : null))
        };

        var onlyGrid = Request.Headers.ContainsKey(AppConsts.HtmxRequestHeader) && !Request.Headers.ContainsKey(AppConsts.HtmxHistoryRestoreHeader);

        Response.Headers.Vary = AppConsts.HtmxRequestHeader;

        return onlyGrid ? PartialView("_SkinGrid", model) : View(model);
    }

    [HttpPost("/skins/{skinId}/buy")]
    [Authorize]
    public async Task<IActionResult> Buy(string skinId, string? returnUrl, CancellationToken cancellationToken)
    {
        var communityId = currentUser.CommunityId;
        if (communityId == null)
            return Unauthorized();

        var result = await skinShopService.BuyAsync(communityId.Value, currentUser.Name, skinId, cancellationToken);

        return Back(result, "Skin bought and equipped.", returnUrl);
    }

    [HttpPost("/skins/{skinId}/equip")]
    [Authorize]
    public async Task<IActionResult> Equip(string skinId, string? returnUrl, CancellationToken cancellationToken)
    {
        var communityId = currentUser.CommunityId;
        if (communityId == null)
            return Unauthorized();

        var result = await skinShopService.EquipAsync(communityId.Value, skinId, CoreConsts.SkinSourceSite, cancellationToken);

        return Back(result, "Skin equipped.", returnUrl);
    }

    [HttpPost("/skins/unequip")]
    [Authorize]
    public async Task<IActionResult> Unequip(string? weapon, string? returnUrl, CancellationToken cancellationToken)
    {
        var communityId = currentUser.CommunityId;
        if (communityId == null)
            return Unauthorized();

        var result = await skinShopService.UnequipAsync(communityId.Value, weapon, cancellationToken);

        return Back(result, "The weapon is back to its original look.", returnUrl);
    }

    private IActionResult Back(SkinShopResult result, string message, string? returnUrl)
    {
        if (result.Succeeded)
            TempData[AppConsts.SkinMessageTempData] = message;
        else
            TempData["Error"] = SkinShopMessages.For(result.Status);

        var local = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.StartsWith(AppConsts.SkinShopUrl, StringComparison.Ordinal);

        return Redirect(local ? returnUrl! : AppConsts.SkinShopUrl);
    }
}
