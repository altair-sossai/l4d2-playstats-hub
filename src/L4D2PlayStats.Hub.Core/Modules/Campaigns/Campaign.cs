namespace L4D2PlayStats.Hub.Core.Modules.Campaigns;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<CampaignMap> Maps { get; set; } = [];
}