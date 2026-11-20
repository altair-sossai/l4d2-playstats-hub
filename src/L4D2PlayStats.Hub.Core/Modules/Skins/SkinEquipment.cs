namespace L4D2PlayStats.Hub.Core.Modules.Skins;

public class SkinEquipment
{
    public int Id { get; set; }
    public long CommunityId { get; set; }
    public string Weapon { get; set; } = null!;
    public string SkinId { get; set; } = null!;
    public string? Source { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}