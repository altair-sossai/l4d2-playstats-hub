using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Skins;
using Microsoft.AspNetCore.Mvc;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[RequireModule(HubModule.SkinShop)]
public class PlayerSkinsApiController(ISkinShopService skinShopService) : ControllerBase
{
    [HttpGet("api/players/skins")]
    public async Task<IActionResult> Skins(CancellationToken cancellationToken)
    {
        var players = await skinShopService.GetAllPlayerSkinsAsync(cancellationToken);

        return Ok(players.Select(player => new
        {
            communityId = player.CommunityId.ToString(),
            skins = player.Owned.Order(),
            equipped = player.Equipped.OrderBy(pair => pair.Key).Select(pair => new { weapon = pair.Key, skin = pair.Value })
        }));
    }

    [HttpGet("api/players/{communityId:long}/wallet")]
    public async Task<IActionResult> Wallet(long communityId, CancellationToken cancellationToken)
    {
        var wallet = await skinShopService.GetWalletAsync(communityId, cancellationToken);

        return Ok(new { mvpSiDamage = wallet.MvpSiDamage, mvpCommon = wallet.MvpCommon, stars = wallet.Stars, spent = wallet.Spent, balance = wallet.Balance });
    }

    [HttpPost("api/players/{communityId:long}/skins/equip")]
    [GameServerAuthorize]
    public async Task<IActionResult> Equip(long communityId, [FromBody] EquipSkinCommand? command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command?.Weapon))
            return BadRequest(new { error = "A weapon is required" });

        var result = string.IsNullOrWhiteSpace(command.Skin)
            ? await skinShopService.UnequipAsync(communityId, command.Weapon, cancellationToken)
            : await skinShopService.EquipAsync(communityId, command.Skin, CoreConsts.SkinSourceGame, cancellationToken);

        return Ok(new { status = result.Status, succeeded = result.Succeeded });
    }
}
