namespace L4D2PlayStats.Hub.Core.Modules.Skins;

public class SkinPriceOverride
{
    public string SkinId { get; set; } = null!;
    public int Price { get; set; }
    public long? UpdatedByCommunityId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}