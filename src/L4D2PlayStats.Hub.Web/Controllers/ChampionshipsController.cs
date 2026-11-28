using System.Text;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Branding;
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
    IChampionshipQueryService queryService,
    IChampionshipJoinService joinService,
    ChampionshipImageStore imageStore,
    ISiteClock siteClock,
    ICurrentUser currentUser) : HubControllerBase
{
    [HttpGet(AppConsts.ChampionshipFlagsUrl + "/{teamId:int}")]
    [ResponseCache(Duration = AppConsts.ChampionshipImageCacheInSeconds, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> Flag(int teamId, CancellationToken cancellationToken)
    {
        var flag = await queryService.FlagAsync(teamId, cancellationToken);

        if (flag == null || (flag.Status == ChampionshipStatus.Draft && !await currentUser.IsAdministratorAsync()))
            return NotFound();

        return ImageFile(flag.ChampionshipId, flag.Flag);
    }

    [HttpGet(AppConsts.ChampionshipFlagsUrl + "/{teamId:int}/pending")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> PendingFlag(int teamId, CancellationToken cancellationToken)
    {
        var flag = await queryService.FlagAsync(teamId, cancellationToken);
        var me = currentUser.CommunityId;

        if (flag == null || !(await currentUser.IsAdministratorAsync() || me != null && (flag.Captain == me || flag.ViceCaptain == me)))
            return NotFound();

        return ImageFile(flag.ChampionshipId, flag.PendingFlag);
    }

    private static readonly string[] Tabs =
    [
        AppConsts.ChampionshipRulesTab, AppConsts.ChampionshipTeamsTab, AppConsts.ChampionshipDraftTab, AppConsts.ChampionshipStandingsTab,
        AppConsts.ChampionshipBracketTab, AppConsts.ChampionshipGamesTab, AppConsts.ChampionshipStatisticsTab
    ];

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Championships = AppConsts.ActiveMenu;

        base.OnActionExecuting(context);
    }

    [HttpGet(AppConsts.ChampionshipsUrl)]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var current = await championshipService.ActiveAsync(cancellationToken);
        var history = await championshipService.HistoryAsync(page, AppConsts.ChampionshipsPageSize, cancellationToken);

        return View(new ChampionshipsIndexModel(current, history, await siteClock.ZoneAsync(cancellationToken)));
    }

    [HttpGet(AppConsts.ChampionshipImagesUrl + "/{id:int}/{file}")]
    [ResponseCache(Duration = AppConsts.ChampionshipImageCacheInSeconds, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> Image(int id, string file, CancellationToken cancellationToken)
    {
        if (!(await championshipService.IsPublicAsync(id, cancellationToken) || await currentUser.IsAdministratorAsync()))
            return NotFound();

        return ImageFile(id, file);
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
        var url = $"{Request.Scheme}://{Request.Host}{ChampionshipUrls.Series(championship.Slug, series.Id)}";
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
    [HttpPost(AppConsts.ChampionshipsUrl + "/{slug}/join")]
    public Task<IActionResult> Join(string slug, string? message, CancellationToken cancellationToken)
    {
        return Registration(slug, id => joinService.AskAsync(id, currentUser.CommunityId!.Value, message, cancellationToken));
    }

    [Authorize]
    [HttpPost(AppConsts.ChampionshipsUrl + "/{slug}/join/cancel")]
    public Task<IActionResult> CancelJoin(string slug, CancellationToken cancellationToken)
    {
        return Registration(slug, id => joinService.CancelAsync(id, currentUser.CommunityId!.Value, cancellationToken));
    }

    private async Task<IActionResult> Registration(string slug, Func<int, Task> action)
    {
        var championship = await FindAsync(slug, HttpContext.RequestAborted);

        if (championship == null)
            return NotFound();

        return await RunAsync<ChampionshipException>(() => action(championship.Id), () => Redirect($"{ChampionshipUrls.Page(championship.Slug)}#{AppConsts.ChampionshipJoinSection}"));
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

    private IActionResult ImageFile(int championshipId, string? file)
    {
        var path = imageStore.PathOf(championshipId, file);
        var contentType = ImageUploads.ContentType(file);

        if (path == null || contentType == null)
            return NotFound();

        Response.Headers.XContentTypeOptions = AppConsts.NoSniff;

        return PhysicalFile(path, contentType);
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
    }
}
