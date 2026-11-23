namespace L4D2PlayStats.Hub.Web.Models.Api;

// One line of a map: the player of round half Half (0 or 1) as survivor or infected, to be replaced.
public class ReplacePlayerCommand
{
    public int? Half { get; set; }
    public bool? Survivor { get; set; }
    public string? Replaced { get; set; }
    public string? Replacement { get; set; }
    public string? ReplacementName { get; set; }
}
