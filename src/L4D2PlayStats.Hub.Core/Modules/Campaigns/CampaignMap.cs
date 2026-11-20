namespace L4D2PlayStats.Hub.Core.Modules.Campaigns;

public class CampaignMap
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string MapName { get; set; } = null!;
    public int Order { get; set; }
}