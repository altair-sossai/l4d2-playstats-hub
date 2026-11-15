using L4D2PlayStats.Hub.Web.Models.Api;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

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
