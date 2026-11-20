namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface ICampaignImageService
{
    string NoImage { get; }

    string Url(string? campaign);
}