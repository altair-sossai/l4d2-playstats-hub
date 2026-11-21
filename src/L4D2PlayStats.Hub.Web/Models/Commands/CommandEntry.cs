using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;

namespace L4D2PlayStats.Hub.Web.Models.Commands;

public record CommandEntry
{
    public string Id { get; init; } = string.Empty;
    public string? Title { get; init; }
    public List<string> Names { get; init; } = [];
    public List<string> Syntax { get; init; } = [];
    public List<string> Who { get; init; } = [];
    public List<string> When { get; init; } = [];
    public HubModule? Module { get; init; }
    public bool Admin { get; init; }
    public string Description { get; init; } = string.Empty;
    public List<CommandExample> Examples { get; init; } = [];
    public List<string> Notes { get; init; } = [];
}
