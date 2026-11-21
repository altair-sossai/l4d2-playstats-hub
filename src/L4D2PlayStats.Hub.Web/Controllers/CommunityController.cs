using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Patents.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Community;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.CommunityRanking)]
[AutoValidateAntiforgeryToken]
public class CommunityController(
    ISettingsService settingsService,
    ICommunityRankingService communityRankingService,
    ICommunityVoteService communityVoteService,
    IRankingService rankingService,
    ICommunityPlayerService communityPlayerService,
    ISiteClock siteClock,
    IStringLocalizer<SharedResource> localizer,
    ICurrentUser currentUser,
    IPatentService patentService) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Community = AppConsts.ActiveMenu;
    }

    [HttpGet("/community")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy, Tags = [AppConsts.CommunityCacheTag])]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);

        var patents = modules.IsEnabled(HubModule.Ranking);
        var entries = await communityRankingService.PageAsync(page, AppConsts.CommunityRankingPageSize, cancellationToken);
        var experience = patents ? await ExperienceAsync(cancellationToken) : null;
        var listed = new PagedListModel<CommunityRankingEntry>(entries);
        var scale = await patentService.ScaleAsync(cancellationToken);

        var model = new CommunityRankingViewModel
        {
            Players = listed.Map(entry => new CommunityRankingRow(entry, experience == null ? null : scale.For(experience.GetValueOrDefault(entry.CommunityId)))),
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
        var moderate = await currentUser.IsAdministratorAsync();
        var showVoters = settings.ShowVoterNames || moderate;
        var voterId = showVoters ? voter : null;
        var votes = await communityRankingService.VotesPageAsync(voterId, player, page, CoreConsts.CommunityVotesPageSize, cancellationToken);
        var participants = await communityRankingService.VoteParticipantsAsync(cancellationToken);
        var filtered = new[] { voterId, player }.Where(id => id != null).Select(id => id!.Value).ToList();
        var names = await communityPlayerService.NamesAsync(filtered, cancellationToken);
        var rating = player == null ? null : await communityRankingService.RatingAsync(player.Value, cancellationToken);

        var model = new CommunityVotesViewModel
        {
            Votes = votes,
            VoterId = voterId,
            VoterName = voterId == null ? null : names[voterId.Value],
            PlayerId = player,
            PlayerName = player == null ? null : names[player.Value],
            PlayerRating = rating,
            Participants = participants,
            ShowVoters = showVoters,
            Moderate = moderate,
            TimeZone = await siteClock.ZoneAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpGet(AppConsts.CommunityPlayersSearchUrl)]
    public async Task<IActionResult> Players(string? role, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var voters = string.Equals(role, AppConsts.CommunityVoterRole, StringComparison.OrdinalIgnoreCase);
        var size = CoreConsts.PlayerDirectoryPageSize;
        var hidden = voters && !settings.ShowVoterNames && !await currentUser.IsAdministratorAsync();
        var participants = hidden ? Page<CommunityVoteParticipant>.Empty(size) : await communityRankingService.ParticipantsAsync(voters, PlayerSearch.Of(q), page, size, cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);
        var label = voters ? localizer["Last vote"].Value : localizer["Last compared"].Value;

        var players = participants.Items.Select(participant => new
        {
            communityId = participant.CommunityId.ToString(),
            name = participant.Name,
            steamId = SteamIds.SteamId(participant.CommunityId),
            avatarUrl = PlayerUrls.Avatar(participant.CommunityId),
            lastSeen = Display.DateTime((voters ? participant.LastVotedAtUtc : participant.LastComparedAtUtc)!.Value, timeZone),
            lastSeenLabel = label
        });

        return Ok(new { page = participants.Number, total = participants.Total, hasMore = participants.HasMore, players });
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
            TempData[AppConsts.ErrorTempData] = refusal;

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
