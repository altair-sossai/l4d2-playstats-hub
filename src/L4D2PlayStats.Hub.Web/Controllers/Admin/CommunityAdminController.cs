using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
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
    ICommunityRankingService communityRankingService,
    ICommunityModerationService moderationService,
    ICurrentUser currentUser) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Admin = "active";
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var size = AppConsts.CommunityVotersPageSize;
        var total = await communityRankingService.VoterCountAsync(cancellationToken);
        var current = Paging.Page(page, total, size);
        var voters = await communityRankingService.VotersAsync(Paging.Skip(current, size), size, cancellationToken);

        var model = new CommunityAdminViewModel
        {
            Settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken),
            Voters = new PagedListModel<CommunityVoter>(voters, current, size, total),
            Blocked = await moderationService.BlockedAsync(cancellationToken),
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

    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        TempData["Reset"] = await communityRankingService.ResetAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("voters/delete")]
    public async Task<IActionResult> RemoveVoter(string? communityId, CancellationToken cancellationToken)
    {
        var parsed = SteamIdParser.Parse(communityId);

        if (parsed != null)
            TempData["Removed"] = await communityRankingService.RemoveVoterAsync(parsed.Value, cancellationToken);
        else
            TempData["Error"] = "That is not a valid SteamID64.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("voters/block")]
    public async Task<IActionResult> BlockVoter(string? communityId, string? returnUrl, CancellationToken cancellationToken)
    {
        var parsed = SteamIdParser.Parse(communityId);

        if (parsed == null)
        {
            TempData["Error"] = "That is not a valid SteamID64.";

            return RedirectToAction(nameof(Index));
        }

        var blocked = await moderationService.BlockAsync(parsed.Value, currentUser.CommunityId, cancellationToken);

        if (IsReturn(returnUrl))
            return LocalRedirect(returnUrl!);

        TempData[blocked ? "Blocked" : "AlreadyBlocked"] = true;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("voters/unblock")]
    public async Task<IActionResult> UnblockVoter(string? communityId, CancellationToken cancellationToken)
    {
        var parsed = SteamIdParser.Parse(communityId);

        if (parsed != null && await moderationService.UnblockAsync(parsed.Value, cancellationToken))
            TempData["Unblocked"] = true;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("votes/delete")]
    public async Task<IActionResult> RemoveVote(int voteId, string? returnUrl, CancellationToken cancellationToken)
    {
        var removed = await moderationService.RemoveVoteAsync(voteId, cancellationToken);

        if (IsReturn(returnUrl))
            return LocalRedirect(returnUrl!);

        TempData["VoteRemoved"] = removed;

        return RedirectToAction(nameof(Index));
    }

    private bool IsReturn(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl);
    }
}
