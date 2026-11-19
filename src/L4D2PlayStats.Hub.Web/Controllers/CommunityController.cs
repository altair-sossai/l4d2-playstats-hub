using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Patents.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Community;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

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
    IStringLocalizer<SharedResource> localizer,
    ICurrentUser currentUser) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Community = "active";
    }

    [HttpGet("/community")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);

        var patents = modules.IsEnabled(HubModule.Ranking);
        var entries = await communityRankingService.ListAsync(cancellationToken);
        var experience = patents ? await ExperienceAsync(cancellationToken) : null;
        var listed = new PagedListModel<CommunityRankingEntry>(entries, page, AppConsts.CommunityRankingPageSize);
        var rows = listed.Items.Select(entry => new CommunityRankingRow(entry, experience == null ? null : PatentCatalog.For(experience.GetValueOrDefault(entry.CommunityId)))).ToList();

        var model = new CommunityRankingViewModel
        {
            Players = new PagedListModel<CommunityRankingRow>(rows, listed.Page, listed.PageSize, listed.TotalItems),
            MinimumComparisons = settings.MinimumComparisonsToBeListed,
            MinimumMatches = settings.MinimumMatchesToBeCandidate,
            Settings = settings,
            Patents = patents
        };

        return View(model);
    }

    [HttpGet(AppConsts.CommunityVotesPage)]
    public async Task<IActionResult> Votes(long? voter, long? player, int page, CancellationToken cancellationToken)
    {
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
            TimeZone = await liveViewService.TimeZoneAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpGet(AppConsts.CommunityPlayersSearchUrl)]
    public async Task<IActionResult> Players(string? role, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var voters = string.Equals(role, AppConsts.CommunityVoterRole, StringComparison.OrdinalIgnoreCase);
        IReadOnlyList<CommunityVoteParticipant> participants = voters && !settings.ShowVoterNames ? [] : await communityRankingService.VoteParticipantsAsync(cancellationToken);
        var timeZone = await liveViewService.TimeZoneAsync(cancellationToken);
        var term = q?.Trim();
        var steamId = SteamIdParser.Parse(term);
        var size = CoreConsts.PlayerDirectoryPageSize;

        var matches = participants
            .Select(participant => new { Participant = participant, Last = voters ? participant.LastVotedAtUtc : participant.LastComparedAtUtc })
            .Where(entry => entry.Last != null)
            .Where(entry => string.IsNullOrEmpty(term)
                            || entry.Participant.CommunityId == steamId
                            || entry.Participant.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase)
                            || entry.Participant.CommunityId.ToString().Contains(term, StringComparison.Ordinal))
            .OrderByDescending(entry => entry.Last)
            .ToList();

        var current = Math.Max(1, page);
        var players = matches.Skip((current - 1) * size).Take(size).Select(entry => new
        {
            communityId = entry.Participant.CommunityId.ToString(),
            name = entry.Participant.Name,
            steamId = SteamIds.SteamId(entry.Participant.CommunityId),
            avatarUrl = $"{AppConsts.AvatarUrl}{entry.Participant.CommunityId}",
            lastSeen = LiveDisplay.ShortDateTime(entry.Last!.Value, timeZone),
            lastSeenLabel = voters ? localizer["Last vote"].Value : localizer["Last compared"].Value
        });

        return Ok(new { page = current, total = matches.Count, hasMore = matches.Count > current * size, players });
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
