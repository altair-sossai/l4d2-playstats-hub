using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Branding;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Championships;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Services;

public class ChampionshipImageService(IChampionshipService championshipService, ChampionshipImageStore store) : IChampionshipImageService
{
    public async Task ValidateAsync(int championshipId, ChampionshipImagesUpload upload, CancellationToken cancellationToken = default)
    {
        var current = await championshipService.ImagesAsync(championshipId, cancellationToken) ?? throw new ChampionshipException("The championship was not found.");

        await CheckAsync(current, upload, cancellationToken);
    }

    public async Task SaveAsync(int championshipId, ChampionshipImagesUpload upload, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var current = await championshipService.ImagesAsync(championshipId, cancellationToken) ?? throw new ChampionshipException("The championship was not found.");

        await CheckAsync(current, upload, cancellationToken);

        var saved = new List<string>();

        try
        {
            var banner = upload.Banner is { Length: > 0 } ? await WriteAsync(championshipId, AppConsts.ChampionshipBannerImage, upload.Banner, saved, cancellationToken) : upload.RemoveBanner ? null : current.Banner;
            var logo = upload.Logo is { Length: > 0 } ? await WriteAsync(championshipId, AppConsts.ChampionshipLogoImage, upload.Logo, saved, cancellationToken) : upload.RemoveLogo ? null : current.Logo;
            var gallery = current.Gallery.Where(image => !upload.RemoveGallery.Contains(image)).ToList();

            foreach (var file in upload.NewGallery)
                gallery.Add(await WriteAsync(championshipId, AppConsts.ChampionshipGalleryImage, file, saved, cancellationToken));

            var images = new ChampionshipImageSet(banner, logo, gallery);
            var previous = await championshipService.SetImagesAsync(championshipId, images, updatedBy, cancellationToken);
            var kept = images.Files.ToHashSet();

            foreach (var file in previous.Files.Where(file => !kept.Contains(file)))
                store.Delete(championshipId, file);
        }
        catch
        {
            foreach (var file in saved)
                store.Delete(championshipId, file);

            throw;
        }
    }

    public void DeleteAll(int championshipId)
    {
        store.DeleteAll(championshipId);
    }

    private static async Task CheckAsync(ChampionshipImageSet current, ChampionshipImagesUpload upload, CancellationToken cancellationToken)
    {
        foreach (var file in upload.Files)
        {
            var error = await ImageUploads.CheckAsync(file, AppConsts.ChampionshipImageMaxBytes, cancellationToken);

            if (error == ImageUploadError.TooLarge)
                throw new ChampionshipException("Nothing was saved: an image is larger than the limit.");

            if (error != null)
                throw new ChampionshipException("Nothing was saved: the images must be PNG, JPG, WEBP or GIF files.");
        }

        if (current.Gallery.Count(image => !upload.RemoveGallery.Contains(image)) + upload.NewGallery.Count > CoreConsts.ChampionshipGalleryMaxImages)
            throw new ChampionshipException("Nothing was saved: the gallery is full. Remove an image before adding another.");
    }

    private async Task<string> WriteAsync(int championshipId, string kind, IFormFile file, List<string> saved, CancellationToken cancellationToken)
    {
        await using var content = file.OpenReadStream();

        var name = await store.SaveAsync(championshipId, kind, content, Path.GetExtension(file.FileName), cancellationToken);

        saved.Add(name);

        return name;
    }
}
