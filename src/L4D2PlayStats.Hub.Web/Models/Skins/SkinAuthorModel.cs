using L4D2PlayStats.Hub.Core.Modules.Skins;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinAuthorModel(string name, string? profileUrl, IReadOnlyList<Skin> skins)
{
    public string Name { get; } = name;
    public string? ProfileUrl { get; } = profileUrl;
    public IReadOnlyList<Skin> Skins { get; } = skins;
}
