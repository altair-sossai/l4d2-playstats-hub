using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerCampaignRowModel(string name, PlayerCampaign? first, PlayerCampaign? second)
{
    public string Name { get; } = name;
    public PlayerCampaign? First { get; } = first;
    public PlayerCampaign? Second { get; } = second;
    public int Games => (First?.Games ?? 0) + (Second?.Games ?? 0);
}
