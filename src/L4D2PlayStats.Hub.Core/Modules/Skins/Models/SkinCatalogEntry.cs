using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Models;

public class SkinCatalogEntry
{
    public string? Id { get; set; }
    public string? Weapon { get; set; }
    public SkinRarity? Rarity { get; set; }
    public int? Price { get; set; }
    public string? WorkshopId { get; set; }
    public int Shots { get; set; }
    public SkinCatalogAuthor? Author { get; set; }
    public Dictionary<string, string?>? Title { get; set; }
    public Dictionary<string, string?>? Description { get; set; }
    public string? OriginalTitle { get; set; }
    public string? OriginalDescription { get; set; }
}
