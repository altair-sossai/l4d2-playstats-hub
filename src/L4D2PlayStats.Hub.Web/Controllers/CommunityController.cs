using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Services;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Models.Community;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.CommunityRanking)]
[AutoValidateAntiforgeryToken]
public class CommunityController(
    ISettingsService settingsService,
    ICommunityRankingService communityRankingService,
    ICommunityVoteService communityVoteService,
    ICurrentUser currentUser) : Controller
{
    [HttpGet("/community")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);

        var model = new CommunityRankingViewModel
        {
            Players = await communityRankingService.ListAsync(cancellationToken),
            MinimumComparisons = settings.MinimumComparisonsToBeListed,
            PlayerPages = modules.IsEnabled(HubModule.Ranking),
            Highlight = currentUser.CommunityId
        };

        return View(model);
    }

    [HttpGet(AppConsts.CommunityVotePage)]
    public async Task<IActionResult> Vote(CancellationToken cancellationToken)
    {
        return View(await ModelAsync(null, cancellationToken));
    }

    [HttpPost(AppConsts.CommunityVotePage)]
    public async Task<IActionResult> Submit(CommunityVoteForm form, CancellationToken cancellationToken)
    {
        var voterId = currentUser.CommunityId;

        var refusal = voterId == null
            ? null
            : Refusal(await communityVoteService.VoteAsync(voterId.Value, form.Left, form.Right, form.Outcome, cancellationToken));

        if (Request.Headers.ContainsKey(AppConsts.HtmxRequestHeader))
            return PartialView("_Ballot", await ModelAsync(refusal, cancellationToken));

        if (refusal != null)
            TempData["Error"] = refusal;

        return Redirect(AppConsts.CommunityVotePage);
    }

    private async Task<CommunityVoteViewModel> ModelAsync(string? refusal, CancellationToken cancellationToken)
    {
        var voterId = currentUser.CommunityId;

        return new CommunityVoteViewModel
        {
            Ballot = voterId == null ? null : await communityVoteService.BallotAsync(voterId.Value, cancellationToken),
            Refusal = refusal
        };
    }

    private static string? Refusal(CommunityVoteResult result)
    {
        return result switch
        {
            CommunityVoteResult.NotOffered => "That pair is not the one waiting for your answer. Here is the current one.",
            CommunityVoteResult.InvalidOutcome => "Choose one of the answers.",
            _ => null
        };
    }
}
