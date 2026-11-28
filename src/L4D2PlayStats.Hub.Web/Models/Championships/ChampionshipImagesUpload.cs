namespace L4D2PlayStats.Hub.Web.Models.Championships;

public class ChampionshipImagesUpload
{
    public IFormFile? Banner { get; set; }
    public bool RemoveBanner { get; set; }
    public IFormFile? Logo { get; set; }
    public bool RemoveLogo { get; set; }
    public List<IFormFile> Gallery { get; set; } = [];
    public List<string> RemoveGallery { get; set; } = [];

    public IEnumerable<IFormFile> Files => new[] { Banner, Logo }.Concat(Gallery).Where(file => file is { Length: > 0 }).Select(file => file!);

    public List<IFormFile> NewGallery => Gallery.Where(file => file is { Length: > 0 }).ToList();
}
