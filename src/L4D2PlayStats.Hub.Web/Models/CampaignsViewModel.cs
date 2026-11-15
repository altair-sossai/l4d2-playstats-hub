using L4D2PlayStats.Hub.Core.Modules.Campaigns;

namespace L4D2PlayStats.Hub.Web.Models;

public class CampaignsViewModel
{
    public IReadOnlyList<Campaign> Campaigns { get; init; } = [];
}
