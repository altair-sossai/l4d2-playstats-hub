namespace L4D2PlayStats.Hub.Web.Models.Bans;

public class BanEditForm
{
    public int Id { get; set; }
    public string? Duration { get; set; }
    public string? EndsAt { get; set; }
    public string? Reason { get; set; }
    public string? ReturnUrl { get; set; }
}
