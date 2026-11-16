namespace L4D2PlayStats.Hub.Web.Services;

public class CampaignImageService(IWebHostEnvironment environment) : ICampaignImageService
{
    public string Url(string? campaign)
    {
        var id = Id(campaign);

        if (id.Length == 0 || !environment.WebRootFileProvider.GetFileInfo(Path(id)).Exists)
            id = AppConsts.NoCampaignImage;

        return $"/{Path(id)}";
    }

    private static string Path(string id)
    {
        return $"{AppConsts.CampaignImagesFolder}/{id}{AppConsts.CampaignImageExtension}";
    }

    private static string Id(string? campaign)
    {
        if (string.IsNullOrEmpty(campaign))
            return string.Empty;

        var id = campaign.ToLowerInvariant().Replace(' ', '-');

        return string.Concat(id.Where(character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-'));
    }
}
