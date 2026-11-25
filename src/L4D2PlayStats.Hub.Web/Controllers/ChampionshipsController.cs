using System.Text;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Championships;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Championships)]
[AutoValidateAntiforgeryToken]
public class ChampionshipsController(
    IChampionshipService championshipService,
    IChampionshipLoadService loadService,
    IChampionshipViewService viewService,
    IChampionshipScheduleService scheduleService,
    IChampionshipSeriesService seriesService,
    ISiteClock siteClock,
    ICurrentUser currentUser) : Controller
{
    private static readonly string[] Tabs =
    [
        AppConsts.ChampionshipRulesTab, AppConsts.ChampionshipTeamsTab, AppConsts.ChampionshipDraftTab, AppConsts.ChampionshipStandingsTab,
        AppConsts.ChampionshipBracketTab, AppConsts.ChampionshipGamesTab, AppConsts.ChampionshipStatisticsTab
    ];

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Championships = "active";

        base.OnActionExecuting(context);
    }

    [HttpGet(AppConsts.ChampionshipsUrl)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var championships = await championshipService.ListAsync(true, cancellationToken);

        return View(new ChampionshipsIndexModel(championships, await siteClock.ZoneAsync(cancellationToken)));
    }

    [HttpGet(AppConsts.ChampionshipsUrl + "/{slug}")]
    public Task<IActionResult> Overview(string slug, CancellationToken cancellationToken)
    {
        return Page(slug, AppConsts.ChampionshipOverviewTab, cancellationToken);
    }

    [HttpGet(AppConsts.ChampionshipsUrl + "/{slug}/{tab}")]
    public Task<IActionResult> Tab(string slug, string tab, CancellationToken cancellationToken)
    {
        return Tabs.Contains(tab) ? Page(slug, tab, cancellationToken) : Task.FromResult<IActionResult>(NotFound());
    }

    [HttpGet(AppConsts.ChampionshipsUrl + "/{slug}/series/{seriesId:int}")]
    public async Task<IActionResult> Series(string slug, int seriesId, CancellationToken cancellationToken)
    {
        var championship = await FindAsync(slug, cancellationToken);
        var series = championship?.Stages.SelectMany(stage => stage.Series).FirstOrDefault(item => item.Id == seriesId);

        if (championship == null || series == null)
            return NotFound();

        return View(await viewService.PageAsync(championship, AppConsts.ChampionshipGamesTab, series, cancellationToken));
    }

    [HttpGet(AppConsts.ChampionshipsUrl + "/{slug}/series/{seriesId:int}/calendar.ics")]
    public async Task<IActionResult> Calendar(string slug, int seriesId, CancellationToken cancellationToken)
    {
        var championship = await FindAsync(slug, cancellationToken);
        var series = championship?.Stages.SelectMany(stage => stage.Series).FirstOrDefault(item => item.Id == seriesId);

        if (championship == null || series?.ScheduledAtUtc == null)
            return NotFound();

        var model = await viewService.PageAsync(championship, AppConsts.ChampionshipGamesTab, series, cancellationToken);
        var start = series.ScheduledAtUtc.Value;
        var title = $"{model.TeamName(series.TeamAId)} x {model.TeamName(series.TeamBId)} - {championship.Name}";
        var url = $"{Request.Scheme}://{Request.Host}{AppConsts.ChampionshipsUrl}/{championship.Slug}/series/{series.Id}";
        var calendar = new StringBuilder()
            .AppendLine("BEGIN:VCALENDAR")
            .AppendLine("VERSION:2.0")
            .AppendLine("PRODID:-//L4D2 PlayStats//Championships//EN")
            .AppendLine("BEGIN:VEVENT")
            .AppendLine($"UID:championship-series-{series.Id}@{Request.Host.Host}")
            .AppendLine($"DTSTAMP:{ChampionshipDates.Calendar(DateTime.UtcNow)}")
            .AppendLine($"DTSTART:{ChampionshipDates.Calendar(start)}")
            .AppendLine($"DTEND:{ChampionshipDates.Calendar(start.AddMinutes(AppConsts.ChampionshipCalendarDurationInMinutes))}")
            .AppendLine($"SUMMARY:{Escape(title)}")
            .AppendLine($"DESCRIPTION:{Escape(championship.StreamUrl ?? url)}")
            .AppendLine($"URL:{url}")
            .AppendLine("END:VEVENT")
            .AppendLine("END:VCALENDAR")
            .ToString();

        return File(Encoding.UTF8.GetBytes(calendar), AppConsts.ChampionshipCalendarContentType, $"{championship.Slug}-{series.Id}.ics");
    }

    [Authorize]
    [HttpPost(AppConsts.ChampionshipsUrl + "/{slug}/series/{seriesId:int}/proposals")]
    public async Task<IActionResult> Propose(string slug, int seriesId, string? date, CancellationToken cancellationToken)
    {
        var zone = await siteClock.ZoneAsync(cancellationToken);
        var utc = ChampionshipDates.ToUtc(date, zone);

        return await Act(slug, seriesId, () => utc == null
            ? throw new ChampionshipException("Choose a date in the future.")
            : scheduleService.ProposeAsync(seriesId, utc.Value, currentUser.CommunityId!.Value, cancellationToken));
    }

    [Authorize]
    [HttpPost(AppConsts.ChampionshipsUrl + "/{slug}/series/{seriesId:int}/proposals/{proposalId:int}/answer")]
    public Task<IActionResult> Answer(string slug, int seriesId, int proposalId, bool accept, CancellationToken cancellationToken)
    {
        return Act(slug, seriesId, () => scheduleService.AnswerAsync(proposalId, accept, currentUser.CommunityId!.Value, cancellationToken));
    }

    [Authorize]
    [HttpPost(AppConsts.ChampionshipsUrl + "/{slug}/series/{seriesId:int}/proposals/{proposalId:int}/delete")]
    public Task<IActionResult> DeleteProposal(string slug, int seriesId, int proposalId, CancellationToken cancellationToken)
    {
        return Act(slug, seriesId, () => scheduleService.DeleteAsync(proposalId, currentUser.CommunityId!.Value, false, cancellationToken));
    }

    [Authorize]
    [HttpPost(AppConsts.ChampionshipsUrl + "/{slug}/series/{seriesId:int}/games/{gameId:int}/pick")]
    public Task<IActionResult> Pick(string slug, int seriesId, int gameId, int campaignId, CancellationToken cancellationToken)
    {
        return Act(slug, seriesId, () => seriesService.PickAsync(gameId, campaignId, currentUser.CommunityId!.Value, cancellationToken));
    }

    [Authorize]
    [HttpPost(AppConsts.ChampionshipsUrl + "/{slug}/series/{seriesId:int}/veto")]
    public Task<IActionResult> Veto(string slug, int seriesId, int campaignId, CancellationToken cancellationToken)
    {
        return Act(slug, seriesId, () => seriesService.VetoAsync(seriesId, campaignId, currentUser.CommunityId!.Value, cancellationToken));
    }

    private async Task<IActionResult> Page(string slug, string tab, CancellationToken cancellationToken)
    {
        var championship = await FindAsync(slug, cancellationToken);

        if (championship == null)
            return NotFound();

        return View(tab == AppConsts.ChampionshipOverviewTab ? "Overview" : char.ToUpperInvariant(tab[0]) + tab[1..], await viewService.PageAsync(championship, tab, null, cancellationToken));
    }

    private async Task<Championship?> FindAsync(string slug, CancellationToken cancellationToken)
    {
        var championship = await loadService.LoadBySlugAsync(slug, cancellationToken);

        if (championship == null)
            return null;

        return championship.IsPublic || await currentUser.IsAdministratorAsync() ? championship : null;
    }

    private async Task<IActionResult> Act(string slug, int seriesId, Func<Task> action)
    {
        var championship = await FindAsync(slug, HttpContext.RequestAborted);

        if (championship == null)
            return NotFound();

        try
        {
            await action();
            TempData[AppConsts.SavedTempData] = true;
        }
        catch (ChampionshipException exception)
        {
            TempData[AppConsts.ErrorTempData] = exception.Message;
        }

        return Redirect($"{AppConsts.ChampionshipsUrl}/{championship.Slug}/series/{seriesId}");
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
    }
}
