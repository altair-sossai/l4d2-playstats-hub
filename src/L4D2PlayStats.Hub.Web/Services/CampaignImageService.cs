using System.Text;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class CampaignImageService(IWebHostEnvironment environment) : ICampaignImageService
{
    public string NoImage => $"/{Path(AppConsts.NoCampaignImage)}";

    public string Url(string? campaign)
    {
        var id = Id(campaign);

        if (id.Length == 0 || !environment.WebRootFileProvider.GetFileInfo(Path(id)).Exists)
            return NoImage;

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

        var builder = new StringBuilder();

        foreach (var character in campaign.ToLowerInvariant())
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
                builder.Append(character);
            else if (character is ' ' or '-' && builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');

        return builder.ToString().Trim('-');
    }
}
