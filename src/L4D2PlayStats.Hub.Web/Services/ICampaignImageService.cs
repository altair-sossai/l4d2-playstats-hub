namespace L4D2PlayStats.Hub.Web.Services;

public interface ICampaignImageService
{
    string NoImage { get; }

    string Url(string? campaign);
}
