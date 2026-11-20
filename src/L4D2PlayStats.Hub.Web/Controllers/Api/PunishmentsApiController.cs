using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Punishments.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route("api/punishments")]
[Authorize(Policy = AppConsts.AdministratorPolicy)]
[RequireModule(HubModule.Ranking)]
[ValidateAntiForgeryToken]
[ApiController]
public class PunishmentsApiController(IPunishmentService punishmentService, ISeasonService seasonService, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] PunishmentCommand command, CancellationToken cancellationToken)
    {
        if (!long.TryParse(command.CommunityId, out var communityId) || !SteamIds.IsValid(communityId) || command.LostExperiencePoints <= 0)
            return BadRequest();

        var seasonId = await SeasonIdAsync(command.SeasonId, cancellationToken);
        if (seasonId == null)
            return NotFound();

        await punishmentService.AddAsync(seasonId.Value, communityId, command.LostExperiencePoints, null, currentUser.CommunityId, cancellationToken);

        var totals = await punishmentService.TotalsAsync(seasonId.Value, cancellationToken);

        return Ok(new { communityId = communityId.ToString(), lostExperiencePoints = totals.GetValueOrDefault(communityId), seasonId });
    }

    [HttpDelete("{communityId:long}")]
    public async Task<IActionResult> DeleteAsync(long communityId, [FromQuery] int? season, CancellationToken cancellationToken)
    {
        var seasonId = await SeasonIdAsync(season, cancellationToken);
        if (seasonId == null)
            return NotFound();

        await punishmentService.RemoveAsync(seasonId.Value, communityId, cancellationToken);

        return NoContent();
    }

    private async Task<int?> SeasonIdAsync(int? seasonId, CancellationToken cancellationToken)
    {
        if (seasonId == null)
            return (await seasonService.GetCurrentAsync(cancellationToken)).Id;

        return (await seasonService.FindAsync(seasonId.Value, cancellationToken))?.Id;
    }
}
