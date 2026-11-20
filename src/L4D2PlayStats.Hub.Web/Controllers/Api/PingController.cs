using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

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