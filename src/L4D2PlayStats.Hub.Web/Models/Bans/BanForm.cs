namespace L4D2PlayStats.Hub.Web.Models.Bans;

public class BanForm
{
    public string? CommunityId { get; set; }
    public string? Name { get; set; }
    public string? Duration { get; set; }
    public string? EndsAt { get; set; }
    public string? Reason { get; set; }
    public List<string>? RelatedIds { get; set; }
    public int? RelatedToBanId { get; set; }
    public string? ReturnUrl { get; set; }
}
