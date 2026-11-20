namespace L4D2PlayStats.Hub.Core.Modules.Skins.Models;

public class PlayerSkins
{
    public long CommunityId { get; init; }
    public HashSet<string> Owned { get; init; } = [];
    public Dictionary<string, string> Equipped { get; init; } = [];

    public bool Owns(string skinId)
    {
        return Owned.Contains(skinId);
    }

    public bool IsEquipped(Skin skin)
    {
        return Equipped.TryGetValue(skin.Weapon, out var skinId) && skinId == skin.Id;
    }

    public static PlayerSkins Of(long communityId, IEnumerable<SkinPurchase> purchases, IEnumerable<SkinEquipment> equipment)
    {
        var owned = purchases.Select(purchase => purchase.SkinId).ToHashSet();

        return new PlayerSkins
        {
            CommunityId = communityId,
            Owned = owned,
            Equipped = equipment
                .Where(item => owned.Contains(item.SkinId))
                .ToDictionary(item => item.Weapon, item => item.SkinId)
        };
    }
}