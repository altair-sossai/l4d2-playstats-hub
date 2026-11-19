using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Services;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Patents.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Community;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.CommunityRanking)]
[AutoValidateAntiforgeryToken]
public class CommunityController(
    ISettingsService settingsService,
    ICommunityRankingService communityRankingService,
    ICommunityVoteService communityVoteService,
    IRankingService rankingService,
    ICommunityPlayerService communityPlayerService,
    ILiveViewService liveViewService,
    ICurrentUser currentUser) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Community = "active";
    }

    [HttpGet("/community")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);

        var playerPages = modules.IsEnabled(HubModule.Ranking);
        var entries = await communityRankingService.ListAsync(cancellationToken);
        var experience = playerPages ? await ExperienceAsync(cancellationToken) : null;

        var model = new CommunityRankingViewModel
        {
            Players = [.. entries.Select(entry => new CommunityRankingRow(entry, experience == null ? null : PatentCatalog.For(experience.GetValueOrDefault(entry.CommunityId))))],
            MinimumComparisons = settings.MinimumComparisonsToBeListed,
            MinimumMatches = settings.MinimumMatchesToBeCandidate,
            Settings = settings,
            PlayerPages = playerPages
        };

        return View(model);
    }

    [HttpGet(AppConsts.CommunityVotesPage)]
    public async Task<IActionResult> Votes(long? voter, long? player, int page, CancellationToken cancellationToken)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var voterId = settings.ShowVoterNames ? voter : null;
        var current = Math.Max(1, page);
        var size = CoreConsts.CommunityVotesPageSize;
        var votes = await communityRankingService.LatestVotesAsync(voterId, player, (current - 1) * size, size + 1, cancellationToken);
        var participants = await communityRankingService.VoteParticipantsAsync(cancellationToken);
        var filtered = new[] { voterId, player }.Where(id => id != null).Select(id => id!.Value).ToList();
        var names = await communityPlayerService.NamesAsync(filtered, cancellationToken);
        var rating = player == null ? null : await communityRankingService.RatingAsync(player.Value, cancellationToken);

        var model = new CommunityVotesViewModel
        {
            Votes = [.. votes.Take(size)],
            VoterId = voterId,
            VoterName = voterId == null ? null : names[voterId.Value],
            PlayerId = player,
            PlayerName = player == null ? null : names[player.Value],
            PlayerRating = rating,
            Participants = participants,
            Page = current,
            HasMore = votes.Count > size,
            ShowVoters = settings.ShowVoterNames,
            PlayerPages = modules.IsEnabled(HubModule.Ranking),
            TimeZone = await liveViewService.TimeZoneAsync(cancellationToken)
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
            Refusal = refusal,
            Settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken)
        };
    }

    private async Task<Dictionary<long, decimal>> ExperienceAsync(CancellationToken cancellationToken)
    {
        var players = await rankingService.CurrentAsync(cancellationToken);

        return players.ToDictionary(player => player.CommunityId, player => player.Experience);
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
