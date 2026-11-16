using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route("api/campaigns")]
[ApiController]
public class CampaignsApiController(ICampaignService campaignService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var catalog = await campaignService.GetCatalogAsync(cancellationToken);

        return Ok(catalog.Campaigns.Select(campaign => new { name = campaign.Name, maps = campaign.Maps }));
    }
}
