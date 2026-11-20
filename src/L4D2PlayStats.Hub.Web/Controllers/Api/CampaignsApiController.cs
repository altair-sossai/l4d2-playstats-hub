using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route("api/campaigns")]
public class CampaignsApiController(ICampaignService campaignService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var catalog = await campaignService.GetCatalogAsync(cancellationToken);

        return Ok(catalog.Campaigns.Select(campaign => new { name = campaign.Name, maps = campaign.Maps }));
    }
}
