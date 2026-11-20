namespace L4D2PlayStats.Hub.Core.Modules.Skins;

public class SkinText
{
    public int Id { get; set; }
    public string SkinId { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
}
