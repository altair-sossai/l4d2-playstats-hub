using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;

namespace L4D2PlayStats.Hub.Web.Models.Commands;

public record CommandSection
{
    public string Id { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Intro { get; init; } = string.Empty;
    public HubModule? Module { get; init; }
    public bool Admin { get; init; }
    public List<CommandEntry> Commands { get; init; } = [];

    public string Anchor => $"{AppConsts.CommandsSectionAnchorPrefix}{Id}";
}
