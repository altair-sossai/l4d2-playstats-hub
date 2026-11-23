namespace L4D2PlayStats.Hub.Web.Models.Api;

public class ReplacePlayerCommand
{
    public string? Replaced { get; set; }
    public string? Replacement { get; set; }
    public string? ReplacementName { get; set; }
    public List<int>? FileIds { get; set; }
}
