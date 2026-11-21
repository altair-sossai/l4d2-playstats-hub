using L4D2PlayStats.Hub.Core.Modules.Campaigns;

namespace L4D2PlayStats.Hub.Web.Models;

public class CampaignsViewModel
{
    public PagedListModel<Campaign> Campaigns { get; init; } = null!;
    public string ReturnUrl { get; init; } = AppConsts.AdminCampaignsUrl;
}
