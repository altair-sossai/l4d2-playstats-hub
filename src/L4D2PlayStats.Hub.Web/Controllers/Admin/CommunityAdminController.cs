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
using L4D2PlayStats.Hub.Web.Models.Players;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[RequireModule(HubModule.CommunityRanking)]
[Route("admin/community")]
public class CommunityAdminController(
    HubDbContext dbContext,
    ISettingsService settingsService,
    ICommunityRankingService communityRankingService,
    ICommunityModerationService moderationService,
    ICurrentUser currentUser) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int blockedPage = 1, string? tab = null, CancellationToken cancellationToken = default)
    {
        var size = AppConsts.CommunityVotersPageSize;

        var model = new CommunityAdminViewModel
        {
            Settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken),
            Voters = new PagedListModel<CommunityVoter>(await communityRankingService.VotersAsync(page, size, cancellationToken)),
            Blocked = new PagedListModel<CommunityBlockedVoter>(await moderationService.BlockedAsync(blockedPage, size, cancellationToken)),
            Tab = CommunityAdminUrls.Tab(tab, page, blockedPage),
            Votes = await dbContext.Set<CommunityVote>().CountAsync(cancellationToken),
            RatedPlayers = await dbContext.Set<CommunityRating>().CountAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(CommunityRankingSettings settings, CancellationToken cancellationToken)
    {
        await settingsService.SaveAsync(settings, cancellationToken);
        Saved();

        return Redirect(CommunityAdminUrls.Of(AppConsts.CommunitySettingsTab));
    }

    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild(CancellationToken cancellationToken)
    {
        TempData[AppConsts.RebuiltTempData] = await communityRankingService.RebuildAsync(cancellationToken);

        return Redirect(CommunityAdminUrls.Of(AppConsts.CommunityMaintenanceTab));
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        TempData[AppConsts.ResetTempData] = await communityRankingService.ResetAsync(cancellationToken);

        return Redirect(CommunityAdminUrls.Of(AppConsts.CommunityMaintenanceTab));
    }

    [HttpPost("voters/delete")]
    public async Task<IActionResult> RemoveVoter(PlayerForm form, CancellationToken cancellationToken)
    {
        if (!await ValidAsync(form, cancellationToken))
            return Redirect(CommunityAdminUrls.Of(AppConsts.CommunityVotersTab));

        TempData[AppConsts.RemovedTempData] = await communityRankingService.RemoveVoterAsync(SteamIdParser.Parse(form.CommunityId)!.Value, cancellationToken);

        return BackToTab(form.ReturnUrl, AppConsts.CommunityVotersTab);
    }

    [HttpPost("voters/block")]
    public async Task<IActionResult> BlockVoter(BlockVoterForm form, CancellationToken cancellationToken)
    {
        if (!await ValidAsync(form, cancellationToken))
            return Redirect(CommunityAdminUrls.Of(AppConsts.CommunityVotersTab));

        var voterId = SteamIdParser.Parse(form.CommunityId)!.Value;
        var blocked = await moderationService.BlockAsync(voterId, currentUser.CommunityId, cancellationToken);
        var removed = form.RemoveVotes ? await communityRankingService.RemoveVoterAsync(voterId, cancellationToken) : (int?)null;

        if (IsAdministrationReturn(form.ReturnUrl))
        {
            TempData[blocked ? AppConsts.BlockedTempData : AppConsts.AlreadyBlockedTempData] = true;

            if (removed != null)
                TempData[AppConsts.RemovedTempData] = removed;
        }

        return BackToTab(form.ReturnUrl, AppConsts.CommunityVotersTab);
    }

    [HttpPost("voters/unblock")]
    public async Task<IActionResult> UnblockVoter(PlayerForm form, CancellationToken cancellationToken)
    {
        if (await ValidAsync(form, cancellationToken) && await moderationService.UnblockAsync(SteamIdParser.Parse(form.CommunityId)!.Value, cancellationToken))
            TempData[AppConsts.UnblockedTempData] = true;

        return BackToTab(form.ReturnUrl, AppConsts.CommunityBlockedTab);
    }

    [HttpPost("votes/delete")]
    public async Task<IActionResult> RemoveVote(int voteId, string? returnUrl, CancellationToken cancellationToken)
    {
        var removed = await moderationService.RemoveVoteAsync(voteId, cancellationToken);

        if (IsAdministrationReturn(returnUrl))
            TempData[AppConsts.VoteRemovedTempData] = removed;

        return BackToTab(returnUrl, AppConsts.CommunityVotersTab);
    }

    private IActionResult BackToTab(string? returnUrl, string tab)
    {
        return Back(returnUrl, CommunityAdminUrls.Of(tab));
    }

    private bool IsAdministrationReturn(string? returnUrl)
    {
        return !IsLocal(returnUrl) || returnUrl!.StartsWith(AppConsts.AdminCommunityUrl, StringComparison.OrdinalIgnoreCase);
    }
}
