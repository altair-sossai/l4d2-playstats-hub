using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route("api/admin/players")]
[Authorize(Policy = AppConsts.AdministratorPolicy)]
[ApiController]
public class AdminPlayersApiController(
    IRankingService rankingService,
    IPlayerLookupService playerLookupService,
    IPlayerDirectoryService playerDirectoryService,
    ISiteClock siteClock) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchAsync([FromQuery] string? q, [FromQuery] int page, CancellationToken cancellationToken)
    {
        var result = await playerDirectoryService.SearchAsync(q, page, cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);

        return Ok(new PlayerSearchPageResult(result, timeZone));
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var current = await rankingService.CurrentAsync(cancellationToken);
        var allTime = await rankingService.AllTimeAsync(cancellationToken);
        var currentIds = current.Select(player => player.CommunityId).ToHashSet();
        var allTimeById = allTime.GroupBy(player => player.CommunityId).ToDictionary(group => group.Key, group => group.First());

        var players = current
            .Concat(allTime.Where(player => !currentIds.Contains(player.CommunityId)))
            .Where(player => player.CommunityId > 0)
            .ToList();

        var result = players.Select(player => new
        {
            communityId = player.CommunityId.ToString(CultureInfo.InvariantCulture),
            name = player.MainName,
            player.SteamId,
            position = currentIds.Contains(player.CommunityId) ? player.Position : (int?)null,
            previousNames = PreviousNames(player, allTimeById.GetValueOrDefault(player.CommunityId)),
            avatarUrl = PlayerUrls.Avatar(player.CommunityId)
        });

        return Ok(result);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> LookupAsync([FromQuery] string? id, CancellationToken cancellationToken)
    {
        var player = await playerLookupService.LookupAsync(id, cancellationToken);

        return player == null ? NotFound() : Ok(player);
    }

    private static List<string> PreviousNames(RankingPlayer player, RankingPlayer? allTimePlayer)
    {
        return
        [
            .. new[] { allTimePlayer?.MainName }
                .Concat(player.PreviousNames.Select(previous => previous.Name))
                .Concat((allTimePlayer?.PreviousNames ?? []).Select(previous => previous.Name))
                .OfType<string>()
                .Where(name => !string.IsNullOrWhiteSpace(name) && name != player.MainName)
                .Distinct()
                .Take(AppConsts.PlayerPickerPreviousNames)
        ];
    }
}
