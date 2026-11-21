using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Api;
using Microsoft.AspNetCore.Mvc;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[RequireModule(HubModule.Ranking)]
public class RankingApiController(IRankingService rankingService) : ControllerBase
{
    [HttpGet("api/ranking")]
    public async Task<IActionResult> Ranking([FromQuery] int count = AppConsts.RankingPageSize, CancellationToken cancellationToken = default)
    {
        var ranking = await rankingService.CurrentAsync(cancellationToken);

        return Ok(ranking.Take(Listed(count)).Select(player => new
        {
            communityId = player.CommunityId.ToString(),
            position = player.Position,
            name = player.MainName,
            experience = player.Experience,
            wins = player.Wins,
            loss = player.Loss,
            level = player.Patents.For(player.Experience).Level
        }));
    }

    [HttpGet("api/players/patents")]
    public async Task<IActionResult> Patents([FromQuery] int count = AppConsts.InGameRankingPlayers, CancellationToken cancellationToken = default)
    {
        var ranking = await rankingService.CurrentAsync(cancellationToken);

        return Ok(ranking.Take(Listed(count)).Select(player => new
        {
            communityId = player.CommunityId.ToString(),
            level = player.Patents.For(player.Experience).Level
        }));
    }

    [HttpPost("api/mix")]
    [GameServerAuthorize]
    public async Task<IActionResult> Mix([FromBody] MixCommand? command, [FromQuery] int count = AppConsts.RankingPageSize, CancellationToken cancellationToken = default)
    {
        var requested = (command?.All ?? []).Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id!.Trim()).Distinct().ToList();

        if (requested.Count != CoreConsts.MixPlayers)
            return BadRequest(new { error = "Eight different players are required" });

        var ranking = await rankingService.CurrentAsync(cancellationToken);
        var players = ranking.Take(Listed(count)).ToDictionary(player => player.CommunityId.ToString());
        var available = requested.Where(players.ContainsKey).OrderBy(id => players[id].Position).ToList();

        if (available.Count != CoreConsts.MixPlayers)
            return BadRequest(new { error = "One or more players are not present in the ranking" });

        int[] survivors = [1, 4, 6, 8];
        int[] infecteds = [2, 3, 5, 7];

        return Ok(new
        {
            survivors = survivors.Select(position => MixPlayer(players[available[position - 1]])),
            infecteds = infecteds.Select(position => MixPlayer(players[available[position - 1]]))
        });
    }

    private static object MixPlayer(RankingPlayer player)
    {
        return new { position = player.Position, communityId = player.CommunityId.ToString(), name = player.ForcedName ?? player.Name };
    }

    private static int Listed(int count)
    {
        return Math.Clamp(count, 1, AppConsts.RankingApiMaxPlayers);
    }
}
