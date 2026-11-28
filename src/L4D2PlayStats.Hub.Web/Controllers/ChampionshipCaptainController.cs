using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Championships;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Championships;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace L4D2PlayStats.Hub.Web.Controllers;

[Authorize]
[RequireModule(HubModule.Championships)]
[AutoValidateAntiforgeryToken]
[Route(AppConsts.ChampionshipsUrl + "/{slug}/" + AppConsts.ChampionshipCaptainPath)]
public class ChampionshipCaptainController(
    IChampionshipLoadService loadService,
    IChampionshipViewService viewService,
    IChampionshipCaptainService captainService,
    IChampionshipIdentityService identityService,
    IChampionshipDraftService draftService,
    IChampionshipSeriesService seriesService,
    IChampionshipScheduleService scheduleService,
    ChampionshipFlagStore flagStore,
    ISiteClock siteClock,
    IClock clock,
    ICurrentUser currentUser) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Championships = "active";

        base.OnActionExecuting(context);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string slug, CancellationToken cancellationToken)
    {
        var (championship, team) = await FindAsync(slug, cancellationToken);

        if (championship == null || team == null)
            return NotFound();

        var page = await viewService.PageAsync(championship, AppConsts.ChampionshipCaptainPath, null, cancellationToken);

        return View(new ChampionshipCaptainModel
        {
            Page = page,
            Team = team,
            Captain = team.CaptainCommunityId == Me,
            Tasks = CaptainTasks.Of(championship, team, clock.UtcNow)
        });
    }

    [HttpPost("identity")]
    public async Task<IActionResult> Identity(string slug, string? name, string? tag, IFormFile? flag, CancellationToken cancellationToken)
    {
        return await Act(slug, AppConsts.ChampionshipCaptainTeamTab, async team =>
        {
            string? saved = null;

            if (flag is { Length: > 0 })
                saved = await flagStore.SaveAsync(flag, cancellationToken);

            try
            {
                flagStore.Delete(await identityService.ProposeAsync(team.Id, Me, name, tag, saved, cancellationToken));
            }
            catch (ChampionshipException)
            {
                if (saved != null)
                    flagStore.Delete([saved]);

                throw;
            }
        });
    }

    [HttpPost("identity/cancel")]
    public Task<IActionResult> CancelIdentity(string slug, CancellationToken cancellationToken)
    {
        return Act(slug, AppConsts.ChampionshipCaptainTeamTab, async team => flagStore.Delete(await identityService.CancelAsync(team.Id, Me, cancellationToken)));
    }

    [HttpPost("flag/remove")]
    public Task<IActionResult> RemoveFlag(string slug, CancellationToken cancellationToken)
    {
        return Act(slug, AppConsts.ChampionshipCaptainTeamTab, async team => flagStore.Delete(await identityService.RemoveFlagAsync(team.Id, Me, true, cancellationToken)));
    }

    [HttpPost("vice")]
    public Task<IActionResult> Vice(string slug, long? communityId, CancellationToken cancellationToken)
    {
        return Act(slug, AppConsts.ChampionshipCaptainTeamTab, team => captainService.SetViceAsync(team.Id, Me, communityId is > 0 ? communityId : null, cancellationToken));
    }

    [HttpPost("draft/pick")]
    public Task<IActionResult> Pick(string slug, long communityId, CancellationToken cancellationToken)
    {
        return Act(slug, AppConsts.ChampionshipCaptainDraftTab, team => draftService.CaptainPickAsync(team.ChampionshipId, communityId, Me, cancellationToken));
    }

    [HttpPost("series/{seriesId:int}/veto")]
    public Task<IActionResult> Veto(string slug, int seriesId, int campaignId, CancellationToken cancellationToken)
    {
        return Act(slug, AppConsts.ChampionshipCaptainSeriesTab, _ => seriesService.VetoAsync(seriesId, campaignId, Me, cancellationToken));
    }

    [HttpPost("series/{seriesId:int}/games/{gameId:int}/pick")]
    public Task<IActionResult> Campaign(string slug, int seriesId, int gameId, int campaignId, CancellationToken cancellationToken)
    {
        return Act(slug, AppConsts.ChampionshipCaptainSeriesTab, _ => seriesService.PickAsync(gameId, campaignId, Me, cancellationToken));
    }

    [HttpPost("series/{seriesId:int}/proposals")]
    public async Task<IActionResult> Propose(string slug, int seriesId, string? date, CancellationToken cancellationToken)
    {
        var utc = ChampionshipDates.ToUtc(date, await siteClock.ZoneAsync(cancellationToken));

        return await Act(slug, AppConsts.ChampionshipCaptainSeriesTab, _ => utc == null
            ? throw new ChampionshipException("Choose a date in the future.")
            : scheduleService.ProposeAsync(seriesId, utc.Value, Me, cancellationToken));
    }

    [HttpPost("series/{seriesId:int}/proposals/{proposalId:int}/answer")]
    public Task<IActionResult> Answer(string slug, int seriesId, int proposalId, bool accept, CancellationToken cancellationToken)
    {
        return Act(slug, AppConsts.ChampionshipCaptainSeriesTab, _ => scheduleService.AnswerAsync(proposalId, accept, Me, cancellationToken));
    }

    [HttpPost("series/{seriesId:int}/proposals/{proposalId:int}/delete")]
    public Task<IActionResult> DeleteProposal(string slug, int seriesId, int proposalId, CancellationToken cancellationToken)
    {
        return Act(slug, AppConsts.ChampionshipCaptainSeriesTab, _ => scheduleService.DeleteAsync(proposalId, Me, false, cancellationToken));
    }

    private long Me => currentUser.CommunityId ?? 0;

    private async Task<(Championship? Championship, ChampionshipTeam? Team)> FindAsync(string slug, CancellationToken cancellationToken)
    {
        var championship = await loadService.LoadBySlugAsync(slug, cancellationToken);

        if (championship == null || !championship.IsPublic || championship.IsOver || currentUser.CommunityId == null)
            return (null, null);

        return (championship, championship.Teams.FirstOrDefault(team => team.IsLeader(currentUser.CommunityId.Value)));
    }

    private async Task<IActionResult> Act(string slug, string tab, Func<ChampionshipTeam, Task> action)
    {
        var (championship, team) = await FindAsync(slug, HttpContext.RequestAborted);

        if (championship == null || team == null)
            return NotFound();

        try
        {
            await action(team);
            TempData[AppConsts.SavedTempData] = true;
        }
        catch (ChampionshipException exception)
        {
            TempData[AppConsts.ErrorTempData] = exception.Message;
        }

        return Redirect(AdminTabs.Url($"{AppConsts.ChampionshipsUrl}/{championship.Slug}/{AppConsts.ChampionshipCaptainPath}", tab));
    }
}
