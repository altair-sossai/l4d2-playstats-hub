using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Services;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.CommunityRanking)]
[Route("admin/community")]
public class CommunityAdminController(
    HubDbContext dbContext,
    ISettingsService settingsService,
    ICommunityRankingService communityRankingService) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Admin = "active";
        ViewBag.Administration = "active";
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new CommunityAdminViewModel
        {
            Settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken),
            Voters = await communityRankingService.VotersAsync(cancellationToken),
            Votes = await dbContext.Set<CommunityVote>().CountAsync(cancellationToken),
            RatedPlayers = await dbContext.Set<CommunityRating>().CountAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(CommunityRankingSettings settings, CancellationToken cancellationToken)
    {
        settings.Normalize();

        await settingsService.SaveAsync(settings, cancellationToken);
        TempData["Saved"] = true;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild(CancellationToken cancellationToken)
    {
        TempData["Rebuilt"] = await communityRankingService.RebuildAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("voters/delete")]
    public async Task<IActionResult> RemoveVoter(long communityId, CancellationToken cancellationToken)
    {
        if (SteamIds.IsValid(communityId))
            TempData["Removed"] = await communityRankingService.RemoveVoterAsync(communityId, cancellationToken);
        else
            TempData["Error"] = "That is not a valid SteamID64.";

        return RedirectToAction(nameof(Index));
    }
}
