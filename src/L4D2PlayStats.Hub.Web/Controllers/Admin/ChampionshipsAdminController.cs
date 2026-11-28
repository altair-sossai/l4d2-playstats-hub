using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Championships;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[RequireModule(HubModule.Championships)]
[Route(AppConsts.AdminChampionshipsUrl)]
public class ChampionshipsAdminController(
    IChampionshipService championshipService,
    IChampionshipLoadService loadService,
    IChampionshipTeamService teamService,
    IChampionshipDraftService draftService,
    IChampionshipStageService stageService,
    IChampionshipSeriesService seriesService,
    IChampionshipScheduleService scheduleService,
    IChampionshipLogService logService,
    IChampionshipQueryService queryService,
    IChampionshipViewService viewService,
    IChampionshipIdentityService identityService,
    IChampionshipMatchService matchService,
    IChampionshipFlagService flagService,
    IChampionshipImageService imageService,
    IChampionshipJoinService joinService,
    ISiteClock siteClock,
    ICurrentUser currentUser,
    IStringLocalizer<SharedResource> L) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var championships = await championshipService.PageAsync(page, AppConsts.ChampionshipsPageSize, cancellationToken);

        return View(new ChampionshipsAdminIndexModel(new PagedListModel<ChampionshipSummary>(championships), await siteClock.ZoneAsync(cancellationToken), AdminListUrls.Page(AppConsts.AdminChampionshipsUrl, championships.Number)));
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(string? name, ChampionshipTemplate template, CancellationToken cancellationToken)
    {
        try
        {
            var championship = await championshipService.CreateAsync(name ?? string.Empty, Templates(template), Me, cancellationToken);

            Saved();

            return Redirect(Address(championship.Id));
        }
        catch (ChampionshipException exception)
        {
            Error(exception.Message);

            return Redirect(AppConsts.AdminChampionshipsUrl);
        }
    }

    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        if (await championshipService.DeleteAsync(id, cancellationToken))
            imageService.DeleteAll(id);

        Saved();

        return Back(returnUrl, AppConsts.AdminChampionshipsUrl);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Championship(int id, int? stage, int participantsPage = 1, int requestsPage = 1, int teamsPage = 1, int freePage = 1, int gamesPage = 1, int historyPage = 1, CancellationToken cancellationToken = default)
    {
        var championship = await loadService.LoadAsync(id, false, cancellationToken);

        if (championship == null)
            return NotFound();

        var page = await viewService.PageAsync(championship, AppConsts.ChampionshipAdminSettingsTab, null, cancellationToken);
        var participants = championship.Participants.OrderBy(participant => participant.AddedAtUtc).ThenBy(participant => participant.Id).ToList();
        var series = page.AllSeries.Where(item => !item.Bye && (stage == null || item.StageId == stage)).ToList();
        var history = await logService.PageAsync(id, historyPage, AppConsts.ChampionshipHistoryPageSize, cancellationToken);
        var entrants = new Dictionary<int, List<int>>();

        foreach (var item in championship.Stages.Where(item => !item.Generated))
        {
            try
            {
                entrants[item.Id] = await stageService.EntrantsAsync(item.Id, cancellationToken);
            }
            catch (ChampionshipException)
            {
                entrants[item.Id] = [];
            }
        }

        var requests = await joinService.ListAsync(id, cancellationToken);
        var named = history.Items.Where(log => log.CommunityId != null).Select(log => log.CommunityId!.Value)
            .Concat(history.Items.SelectMany(log => (log.Values ?? string.Empty).Split(CoreConsts.ChampionshipLogValueSeparator)).Select(value => long.TryParse(value, out var number) ? number : 0).Where(number => number >= CoreConsts.SteamCommunityIdBase))
            .Concat(requests.Select(request => request.CommunityId))
            .Distinct()
            .ToList();

        page.Names = await queryService.NamesAsync(named, cancellationToken);

        return View(new ChampionshipAdminModel
        {
            Page = page,
            Requests = new PagedListModel<ChampionshipJoinRequest>(requests, requestsPage, AppConsts.ChampionshipRequestsPageSize),
            PendingRequests = requests.Count(request => request.Status == ChampionshipJoinRequestStatus.Pending),
            Participants = new PagedListModel<ChampionshipParticipant>(participants, participantsPage, AppConsts.ChampionshipParticipantsPageSize),
            Teams = new PagedListModel<ChampionshipTeam>(page.Teams, teamsPage, AppConsts.ChampionshipTeamsPageSize),
            Free = new PagedListModel<long>(ChampionshipDraftService.Free(championship), freePage, AppConsts.ChampionshipFreePageSize),
            Series = new PagedListModel<ChampionshipSeries>(series, gamesPage, AppConsts.ChampionshipGamesPageSize),
            History = new PagedListModel<ChampionshipLog>(history),
            Entrants = entrants,
            StageFilter = stage
        });
    }

    [HttpPost("{id:int}")]
    [RequestSizeLimit(AppConsts.ChampionshipSettingsFormMaxBytes)]
    public async Task<IActionResult> SaveSettings(int id, ChampionshipSettingsForm form, ChampionshipImagesUpload images, string? startsOn, string? endsOn, string? suggestedTime, ChampionshipStatus status, string? reason, CancellationToken cancellationToken)
    {
        form.StartsOn = ChampionshipDates.FromDateInput(startsOn);
        form.EndsOn = ChampionshipDates.FromDateInput(endsOn);
        form.SuggestedMinute = ChampionshipDates.FromTimeInput(suggestedTime);

        return await Run(async () =>
        {
            await imageService.ValidateAsync(id, images, cancellationToken);
            await championshipService.UpdateAsync(id, form, Me, cancellationToken);
            await imageService.SaveAsync(id, images, Me, cancellationToken);

            var championship = await loadService.LoadAsync(id, false, cancellationToken);

            if (championship != null && (championship.Status != status || championship.CancelReason != reason?.Trim()))
                await championshipService.SetStatusAsync(id, status, reason, Me, cancellationToken);
        }, null, Address(id, AppConsts.ChampionshipAdminSettingsTab));
    }

    [HttpPost("{id:int}/rules")]
    public Task<IActionResult> SaveRules(int id, string? rules, string? rulesPortuguese, string? rulesSpanish, CancellationToken cancellationToken)
    {
        return Run(() => championshipService.UpdateRulesAsync(id, rules, rulesPortuguese, rulesSpanish, Me, cancellationToken), null, Address(id, AppConsts.ChampionshipAdminRulesTab));
    }

    [HttpPost("{id:int}/participants")]
    public Task<IActionResult> AddParticipants(int id, string? players, string? returnUrl, CancellationToken cancellationToken)
    {
        var ids = (players ?? string.Empty).Split(['\n', '\r', ',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(SteamIdParser.Parse).ToList();

        return Run(async () =>
        {
            if (ids.Count == 0 || ids.Any(value => value == null))
                throw new ChampionshipException(AppConsts.InvalidSteamIdMessage);

            await teamService.AddParticipantsAsync(id, ids.Select(value => value!.Value).ToList(), Me, cancellationToken);
        }, returnUrl, Address(id, AppConsts.ChampionshipAdminParticipantsTab));
    }

    [HttpPost("{id:int}/requests/{requestId:int}/approve")]
    public Task<IActionResult> ApproveRequest(int id, int requestId, string? returnUrl, CancellationToken cancellationToken)
    {
        return Run(() => joinService.ApproveAsync(requestId, Me, cancellationToken), returnUrl, Address(id, AppConsts.ChampionshipAdminParticipantsTab));
    }

    [HttpPost("{id:int}/requests/{requestId:int}/refuse")]
    public Task<IActionResult> RefuseRequest(int id, int requestId, string? returnUrl, CancellationToken cancellationToken)
    {
        return Run(() => joinService.RefuseAsync(requestId, Me, cancellationToken), returnUrl, Address(id, AppConsts.ChampionshipAdminParticipantsTab));
    }

    [HttpPost("{id:int}/participants/{communityId:long}/remove")]
    public Task<IActionResult> RemoveParticipant(int id, long communityId, string? returnUrl, CancellationToken cancellationToken)
    {
        return Run(() => teamService.RemoveParticipantAsync(id, communityId, Me, cancellationToken), returnUrl, Address(id, AppConsts.ChampionshipAdminParticipantsTab));
    }

    [HttpPost("{id:int}/participants/{communityId:long}/pot")]
    public Task<IActionResult> Pot(int id, long communityId, int pot, string? returnUrl, CancellationToken cancellationToken)
    {
        return Run(() => teamService.SetPotAsync(id, communityId, pot, cancellationToken), returnUrl, Address(id, AppConsts.ChampionshipAdminParticipantsTab));
    }

    [HttpPost("{id:int}/teams")]
    public Task<IActionResult> CreateTeam(int id, string? name, string? tag, CancellationToken cancellationToken)
    {
        return Run(() => teamService.CreateTeamAsync(id, name ?? string.Empty, tag, Me, cancellationToken), null, Address(id, AppConsts.ChampionshipAdminTeamsTab));
    }

    [HttpPost("{id:int}/teams/many")]
    public Task<IActionResult> CreateTeams(int id, int count, CancellationToken cancellationToken)
    {
        return Run(() => teamService.CreateTeamsAsync(id, count, L["Team"], Me, cancellationToken), null, Address(id, AppConsts.ChampionshipAdminTeamsTab));
    }

    [HttpPost("{id:int}/teams/{teamId:int}")]
    public Task<IActionResult> UpdateTeam(int id, int teamId, string? name, string? tag, int seed, long? captain, long? viceCaptain, IFormFile? flag, bool removeFlag, CancellationToken cancellationToken)
    {
        return Run(async () =>
        {
            await teamService.UpdateTeamAsync(teamId, name ?? string.Empty, tag, seed, Me, cancellationToken);
            await teamService.SetLeadersAsync(teamId, captain is > 0 ? captain : null, viceCaptain is > 0 ? viceCaptain : null, Me, cancellationToken);

            if (flag is { Length: > 0 })
                await flagService.ApplyAsync(id, flag, saved => identityService.SetFlagAsync(teamId, saved!, Me, cancellationToken), cancellationToken);
            else if (removeFlag)
                flagService.Delete(id, await identityService.RemoveFlagAsync(teamId, Me, false, cancellationToken));
        }, null, Address(id, AppConsts.ChampionshipAdminTeamsTab));
    }

    [HttpPost("{id:int}/teams/{teamId:int}/identity/{operation}")]
    public Task<IActionResult> Identity(int id, int teamId, string operation, string? reason, CancellationToken cancellationToken)
    {
        var back = Address(id, AppConsts.ChampionshipAdminTeamsTab);

        return operation switch
        {
            "approve" => Run(async () => flagService.Delete(id, await identityService.ApproveAsync(teamId, Me, cancellationToken)), null, back),
            "refuse" => Run(async () => flagService.Delete(id, await identityService.RejectAsync(teamId, reason, Me, cancellationToken)), null, back),
            _ => Task.FromResult<IActionResult>(NotFound())
        };
    }

    [HttpPost("{id:int}/teams/{teamId:int}/delete")]
    public async Task<IActionResult> DeleteTeam(int id, int teamId, CancellationToken cancellationToken)
    {
        var files = await identityService.FilesAsync(id, teamId, cancellationToken);

        return await Run(async () =>
        {
            await teamService.DeleteTeamAsync(teamId, Me, cancellationToken);
            flagService.Delete(id, files);
        }, null, Address(id, AppConsts.ChampionshipAdminTeamsTab));
    }

    [HttpPost("{id:int}/teams/{teamId:int}/members")]
    public async Task<IActionResult> AddMember(int id, int teamId, string? player, CancellationToken cancellationToken)
    {
        var back = Address(id, AppConsts.ChampionshipAdminTeamsTab);

        return TryParseSteamId(player, out var communityId) ? await Run(() => teamService.AddMemberAsync(teamId, communityId, Me, cancellationToken), null, back) : Redirect(back);
    }

    [HttpPost("{id:int}/teams/{teamId:int}/members/{communityId:long}/remove")]
    public Task<IActionResult> RemoveMember(int id, int teamId, long communityId, CancellationToken cancellationToken)
    {
        return Run(() => teamService.RemoveMemberAsync(teamId, communityId, Me, cancellationToken), null, Address(id, AppConsts.ChampionshipAdminTeamsTab));
    }

    [HttpPost("{id:int}/draft/{operation}")]
    public Task<IActionResult> Draft(int id, string operation, long? communityId, CancellationToken cancellationToken)
    {
        var back = Address(id, AppConsts.ChampionshipAdminDraftTab);

        return operation switch
        {
            "order" => Run(() => draftService.DrawOrderAsync(id, Me, cancellationToken), null, back),
            "pick" => Run(() => communityId == null ? throw new ChampionshipException("The player is not available in the draft.") : draftService.PickAsync(id, communityId.Value, Me, cancellationToken), null, back),
            "spin" => Run(() => draftService.SpinAsync(id, Me, cancellationToken), null, back),
            "pots" => Run(() => draftService.DrawPotsAsync(id, Me, cancellationToken), null, back),
            "undo" => Run(() => draftService.UndoAsync(id, Me, cancellationToken), null, back),
            "reset" => Run(() => draftService.ResetAsync(id, Me, cancellationToken), null, back),
            _ => Task.FromResult<IActionResult>(NotFound())
        };
    }

    [HttpPost("{id:int}/stages")]
    public Task<IActionResult> AddStage(int id, string? name, ChampionshipStageType type, CancellationToken cancellationToken)
    {
        return Run(() => stageService.AddAsync(id, name ?? string.Empty, type, Me, cancellationToken), null, Address(id, AppConsts.ChampionshipAdminStagesTab));
    }

    [HttpPost("{id:int}/stages/{stageId:int}")]
    public Task<IActionResult> UpdateStage(int id, int stageId, ChampionshipStageForm form, string[]? tiebreaker, CancellationToken cancellationToken)
    {
        form.Tiebreakers = (tiebreaker ?? []).Select(value => Enum.TryParse<ChampionshipTiebreaker>(value, out var parsed) ? (ChampionshipTiebreaker?)parsed : null).Where(value => value != null).Select(value => value!.Value).ToList();

        return Run(() => stageService.UpdateAsync(stageId, form, Me, cancellationToken), null, Address(id, AppConsts.ChampionshipAdminStagesTab));
    }

    [HttpPost("{id:int}/stages/{stageId:int}/{operation}")]
    public async Task<IActionResult> Stage(int id, int stageId, string operation, int? teamId, int? points, string? reason, int? penaltyId, CancellationToken cancellationToken)
    {
        var stages = Address(id, AppConsts.ChampionshipAdminStagesTab);

        return operation switch
        {
            "delete" => await Run(() => stageService.DeleteAsync(stageId, Me, cancellationToken), null, stages),
            "generate" => await Run(() => stageService.GenerateAsync(stageId, Me, cancellationToken), null, stages),
            "reset" => await Run(() => stageService.ResetAsync(stageId, Me, cancellationToken), null, stages),
            "penalty" => await Run(() => stageService.AddPenaltyAsync(stageId, teamId ?? 0, points ?? 0, reason, Me, cancellationToken), null, stages),
            "unpenalize" => await Run(() => stageService.RemovePenaltyAsync(penaltyId ?? 0, Me, cancellationToken), null, stages),
            _ => NotFound()
        };
    }

    [HttpPost("{id:int}/campaigns")]
    public Task<IActionResult> Campaigns(int id, int stageId, string? campaigns, CancellationToken cancellationToken)
    {
        return Run(() => campaigns == AppConsts.ChampionshipClearCampaigns ? stageService.ClearCampaignsAsync(stageId, Me, cancellationToken) : stageService.DrawCampaignsAsync(stageId, Me, cancellationToken), null, Address(id, AppConsts.ChampionshipAdminGamesTab));
    }

    [HttpPost("{id:int}/deadlines")]
    public async Task<IActionResult> Deadlines(int id, int stageId, int round, string? deadline, CancellationToken cancellationToken)
    {
        var zone = await siteClock.ZoneAsync(cancellationToken);

        return await Run(() => stageService.SetRoundDeadlineAsync(stageId, round, ChampionshipDates.ToUtc(deadline, zone), Me, cancellationToken), null, Address(id, AppConsts.ChampionshipAdminGamesTab));
    }

    [HttpPost("{id:int}/series/{seriesId:int}")]
    public async Task<IActionResult> SaveSeries(int id, int seriesId, string? scheduledAt, string? deadline, string? notes, int? winnerTeamId, int? teamAVeto, int? teamBVeto, bool veto, string? returnUrl, CancellationToken cancellationToken)
    {
        var zone = await siteClock.ZoneAsync(cancellationToken);

        return await Run(async () =>
        {
            await seriesService.SetScheduleAsync(seriesId, ChampionshipDates.ToUtc(scheduledAt, zone), ChampionshipDates.ToUtc(deadline, zone), notes, Me, cancellationToken);

            if (veto)
            {
                await seriesService.SetVetoAsync(seriesId, true, teamAVeto is > 0 ? teamAVeto : null, Me, cancellationToken);
                await seriesService.SetVetoAsync(seriesId, false, teamBVeto is > 0 ? teamBVeto : null, Me, cancellationToken);
            }

            await seriesService.WalkoverAsync(seriesId, winnerTeamId is > 0 ? winnerTeamId : null, Me, cancellationToken);
        }, returnUrl, Address(id, AppConsts.ChampionshipAdminGamesTab));
    }

    [HttpGet("{id:int}/series/{seriesId:int}/candidates")]
    public async Task<IActionResult> Candidates(int id, int seriesId, CancellationToken cancellationToken)
    {
        var zone = await siteClock.ZoneAsync(cancellationToken);
        var candidates = await seriesService.CandidatesAsync(seriesId, cancellationToken);

        return Json(candidates.Select(candidate => new
        {
            candidate.MatchId,
            candidate.Linked,
            Label = $"#{candidate.MatchId} · {Display.DateTime(candidate.PlayedAtUtc, zone)} · {candidate.Campaign ?? "?"} · {candidate.TeamAScore.ToString("N0", CultureInfo.CurrentCulture)} × {candidate.TeamBScore.ToString("N0", CultureInfo.CurrentCulture)} · {L["{0}/{1} players", candidate.TeamAPlayers, candidate.TeamBPlayers].Value}"
        }));
    }

    [HttpPost("{id:int}/series/{seriesId:int}/games/{gameId:int}")]
    public async Task<IActionResult> SaveGame(int id, int seriesId, int gameId, int? campaignId, int? originalCampaignId, string? result, int? matchId, int? teamAScore, int? teamBScore, string? playedAt, int? teamAPenalty, int? teamBPenalty, string? notes, string? returnUrl, CancellationToken cancellationToken)
    {
        var zone = await siteClock.ZoneAsync(cancellationToken);

        return await Run(async () =>
        {
            var campaign = campaignId is > 0 ? campaignId : null;

            if (campaign != (originalCampaignId is > 0 ? originalCampaignId : null))
                await seriesService.SetCampaignAsync(gameId, campaign, Me, cancellationToken);

            switch (result)
            {
                case AppConsts.ChampionshipMatchResult:
                    await seriesService.LinkMatchAsync(gameId, matchId ?? throw new ChampionshipException("The match was not found."), Me, cancellationToken);
                    break;
                case AppConsts.ChampionshipTypedResult:
                    await seriesService.SetManualResultAsync(gameId, teamAScore ?? -1, teamBScore ?? -1, ChampionshipDates.ToUtc(playedAt, zone), Me, cancellationToken);
                    break;
                default:
                    await seriesService.ClearResultAsync(gameId, Me, cancellationToken);
                    break;
            }

            if (result is AppConsts.ChampionshipMatchResult or AppConsts.ChampionshipTypedResult)
                await seriesService.SetPenaltiesAsync(gameId, teamAPenalty ?? 0, teamBPenalty ?? 0, notes, Me, cancellationToken);
        }, returnUrl, Address(id, AppConsts.ChampionshipAdminGamesTab));
    }

    [HttpPost("{id:int}/series/{seriesId:int}/games/{gameId:int}/end")]
    public Task<IActionResult> EndGame(int id, int seriesId, int gameId, string? returnUrl, CancellationToken cancellationToken)
    {
        return Run(() => matchService.CloseAsync(gameId, Me, cancellationToken), returnUrl, Address(id, AppConsts.ChampionshipAdminGamesTab));
    }

    [HttpPost("{id:int}/series/{seriesId:int}/proposals/{proposalId:int}/delete")]
    public Task<IActionResult> DeleteProposal(int id, int seriesId, int proposalId, string? returnUrl, CancellationToken cancellationToken)
    {
        return Run(() => scheduleService.DeleteAsync(proposalId, Me ?? 0, true, cancellationToken), returnUrl, Address(id, AppConsts.ChampionshipAdminGamesTab));
    }

    private long? Me => currentUser.CommunityId;

    private List<ChampionshipStageTemplate> Templates(ChampionshipTemplate template)
    {
        var league = new ChampionshipStageTemplate(L["League"], ChampionshipStageType.RoundRobin, 1, 0, 1, ChampionshipCampaignRule.RoundDraw, ChampionshipSeeding.Seeds);
        var groups = new ChampionshipStageTemplate(L["Group stage"], ChampionshipStageType.RoundRobin, 2, 2, 1, ChampionshipCampaignRule.RoundDraw, ChampionshipSeeding.Seeds);
        var playoffs = new ChampionshipStageTemplate(L["Playoffs"], ChampionshipStageType.Knockout, 1, 0, 3, ChampionshipCampaignRule.Picks, ChampionshipSeeding.Standings);

        return template switch
        {
            ChampionshipTemplate.RoundRobin => [league],
            ChampionshipTemplate.RoundRobinAndKnockout => [league with { Qualifiers = 4 }, playoffs],
            ChampionshipTemplate.GroupsAndKnockout => [groups, playoffs],
            _ => [playoffs with { Seeding = ChampionshipSeeding.Random }]
        };
    }

    private Task<IActionResult> Run(Func<Task> action, string? returnUrl, string fallback)
    {
        return RunAsync<ChampionshipException>(action, () => Back(returnUrl, fallback));
    }


    private static string Address(int id, string? tab = null)
    {
        var url = ChampionshipUrls.Admin(id);

        return tab == null ? url : AdminTabs.Url(url, tab);
    }
}
