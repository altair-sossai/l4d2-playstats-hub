using L4D2PlayStats.Hub.Core.Modules.Patents;
using L4D2PlayStats.Hub.Core.Modules.Ranking;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using L4D2PlayStats.Hub.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

public class HeartbeatCommand
{
    public string? Hostname { get; set; }
    public string? Map { get; set; }
    public string? Configuration { get; set; }
    public string? SourceModVersion { get; set; }
    public bool? StatisticsWriting { get; set; }
    public Dictionary<string, string>? Plugins { get; set; }
}

public class StatisticsCommand
{
    public string? FileName { get; set; }
    public string? Content { get; set; }
}

public class MixCommand
{
    public string? Player1 { get; set; }
    public string? Player2 { get; set; }
    public string? Player3 { get; set; }
    public string? Player4 { get; set; }
    public string? Player5 { get; set; }
    public string? Player6 { get; set; }
    public string? Player7 { get; set; }
    public string? Player8 { get; set; }

    public IEnumerable<string?> All => [Player1, Player2, Player3, Player4, Player5, Player6, Player7, Player8];
}

[ApiController]
[Route("api")]
public class PingController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { pong = true, utcNow = DateTime.UtcNow });
    }
}

[ApiController]
[Route("api/servers")]
public class ServersApiController(IGameServerService gameServerService, ISettingsService settingsService) : ControllerBase
{
    [HttpPost("heartbeat")]
    [GameServerAuthorize]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatCommand? command, CancellationToken cancellationToken)
    {
        var server = HttpContext.GameServer();

        var signal = new GameServerSignal
        {
            Hostname = command?.Hostname,
            Map = command?.Map,
            Configuration = command?.Configuration,
            SourceModVersion = command?.SourceModVersion,
            StatisticsWriting = command?.StatisticsWriting,
            Plugins = command?.Plugins ?? []
        };

        await gameServerService.SignalAsync(server, signal, cancellationToken);

        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        return Ok(new
        {
            server = server.Name,
            site = site.Name,
            modules = Enum.GetValues<HubModule>().Where(modules.IsEnabled).Select(module => module.ToString())
        });
    }
}

[ApiController]
[Route("api/statistics")]
public class StatisticsApiController(IStatisticsService statisticsService) : ControllerBase
{
    [HttpPost]
    [GameServerAuthorize]
    public async Task<IActionResult> Upload([FromBody] StatisticsCommand? command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await statisticsService.UploadAsync(HttpContext.GameServer(), command?.FileName, command?.Content, cancellationToken);

            return Ok(new { fileName = result.FileName, mustBeDeleted = result.MustBeDeleted });
        }
        catch (StatisticsRejectedException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}

[ApiController]
[RequireModule(HubModule.Ranking)]
public class RankingApiController(IRankingService rankingService) : ControllerBase
{
    [HttpGet("api/ranking")]
    public async Task<IActionResult> Ranking([FromQuery] int count = AppConsts.RankingPageSize, CancellationToken cancellationToken = default)
    {
        var ranking = await rankingService.CurrentAsync(cancellationToken);

        return Ok(ranking.Take(Math.Clamp(count, 1, 5000)).Select(player => new
        {
            communityId = player.CommunityId.ToString(),
            position = player.Position,
            name = player.MainName,
            experience = player.Experience,
            wins = player.Wins,
            loss = player.Loss,
            level = PatentCatalog.For(player.Experience).Level
        }));
    }

    [HttpGet("api/players/patents")]
    public async Task<IActionResult> Patents([FromQuery] int count = AppConsts.InGameRankingPlayers, CancellationToken cancellationToken = default)
    {
        var ranking = await rankingService.CurrentAsync(cancellationToken);

        return Ok(ranking.Take(Math.Clamp(count, 1, 5000)).Select(player => new
        {
            communityId = player.CommunityId.ToString(),
            level = PatentCatalog.For(player.Experience).Level
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
        var players = ranking.Take(Math.Clamp(count, 1, 5000)).ToDictionary(player => player.CommunityId.ToString());
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
        return new { position = player.Position, communityId = player.CommunityId.ToString(), name = player.Name };
    }
}
