using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Skins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route("api/shop")]
[Authorize]
[ApiController]
[RequireModule(HubModule.SkinShop)]
public class SkinShopApiController(ISkinShopService skinShopService, ISkinService skinService, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("wallet")]
    public async Task<IActionResult> Wallet(CancellationToken cancellationToken)
    {
        var communityId = currentUser.CommunityId;
        if (communityId == null)
            return Unauthorized();

        var wallet = await skinShopService.GetWalletAsync(communityId.Value, cancellationToken);

        return Ok(new { balance = wallet.Balance, stars = wallet.Stars, spent = wallet.Spent });
    }

    [HttpPost("skins/{skinId}/buy")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Buy(string skinId, CancellationToken cancellationToken)
    {
        var communityId = currentUser.CommunityId;
        if (communityId == null)
            return Unauthorized();

        var result = await skinShopService.BuyAsync(communityId.Value, currentUser.Name, skinId, cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("skins/{skinId}/equip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Equip(string skinId, CancellationToken cancellationToken)
    {
        var communityId = currentUser.CommunityId;
        if (communityId == null)
            return Unauthorized();

        var result = await skinShopService.EquipAsync(communityId.Value, skinId, CoreConsts.SkinSourceSite, cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("weapons/{weapon}/unequip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unequip(string weapon, CancellationToken cancellationToken)
    {
        var communityId = currentUser.CommunityId;
        if (communityId == null)
            return Unauthorized();

        var result = await skinShopService.UnequipAsync(communityId.Value, weapon, cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("skins/{skinId}/price")]
    [Authorize(Policy = AppConsts.AdministratorPolicy)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Price(string skinId, [FromBody] SkinPriceCommand command, CancellationToken cancellationToken)
    {
        if (command.Price is < CoreConsts.SkinMinPrice or > CoreConsts.SkinMaxPrice)
            return BadRequest();

        if (!await skinService.SetPriceAsync(skinId, command.Price, currentUser.CommunityId, cancellationToken))
            return NotFound();

        var skin = await skinService.FindAsync(skinId, cancellationToken);
        if (skin == null)
            return NotFound();

        return Ok(new { price = skin.Price, defaultPrice = skin.DefaultPrice, custom = skin.HasCustomPrice });
    }

    [HttpPost("gift")]
    [Authorize(Policy = AppConsts.AdministratorPolicy)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gift([FromBody] SkinOwnershipCommand command, CancellationToken cancellationToken)
    {
        if (!SkinRecipient.TryParse(command.CommunityId, out var communityId) || string.IsNullOrEmpty(command.SkinId))
            return BadRequest();

        var result = await skinShopService.GiftAsync(communityId, command.SkinId, currentUser.CommunityId, currentUser.Name, cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("refund")]
    [Authorize(Policy = AppConsts.AdministratorPolicy)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund([FromBody] SkinOwnershipCommand command, CancellationToken cancellationToken)
    {
        if (!SkinRecipient.TryParse(command.CommunityId, out var communityId) || string.IsNullOrEmpty(command.SkinId))
            return BadRequest();

        var result = await skinShopService.RefundAsync(communityId, command.SkinId, currentUser.CommunityId, cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpPost("refund-all")]
    [Authorize(Policy = AppConsts.AdministratorPolicy)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefundAll(CancellationToken cancellationToken)
    {
        var summary = await skinShopService.RefundAllAsync(currentUser.CommunityId, cancellationToken);

        return Ok(new { purchases = summary.Purchases, gifts = summary.Gifts, stars = summary.Stars, players = summary.Players, failed = summary.Failed });
    }

    private static object ToResponse(SkinShopResult result)
    {
        return new
        {
            status = result.Status,
            succeeded = result.Succeeded,
            price = result.Skin?.Price,
            balance = result.Wallet?.Balance
        };
    }
}
