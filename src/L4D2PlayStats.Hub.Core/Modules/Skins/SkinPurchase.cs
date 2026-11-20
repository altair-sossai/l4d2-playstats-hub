namespace L4D2PlayStats.Hub.Core.Modules.Skins;

public class SkinPurchase
{
    public int Id { get; set; }
    public long CommunityId { get; set; }
    public string SkinId { get; set; } = null!;
    public Skin? Skin { get; set; }
    public int Price { get; set; }
    public int StarsBefore { get; set; }
    public bool Gift { get; set; }
    public string? PlayerName { get; set; }
    public long? GiftedByCommunityId { get; set; }
    public string? GiftedByName { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
}