using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Branding;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class ChampionshipFlagService(ChampionshipImageStore store) : IChampionshipFlagService
{
    public async Task ApplyAsync(int championshipId, IFormFile? upload, Func<string?, Task<List<string>>> apply, CancellationToken cancellationToken = default)
    {
        var saved = upload is { Length: > 0 } ? await SaveAsync(championshipId, upload, cancellationToken) : null;

        try
        {
            Delete(championshipId, await apply(saved));
        }
        catch
        {
            if (saved != null)
                store.Delete(championshipId, saved);

            throw;
        }
    }

    public void Delete(int championshipId, IEnumerable<string> files)
    {
        foreach (var file in files)
            store.Delete(championshipId, file);
    }

    private async Task<string> SaveAsync(int championshipId, IFormFile upload, CancellationToken cancellationToken)
    {
        var error = await ImageUploads.CheckAsync(upload, AppConsts.ChampionshipImageMaxBytes, cancellationToken);

        if (error != null)
            throw new ChampionshipException(error == ImageUploadError.TooLarge ? AppConsts.ChampionshipFlagTooLargeMessage : AppConsts.ChampionshipFlagUnsupportedMessage);

        await using var content = upload.OpenReadStream();

        return await store.SaveAsync(championshipId, AppConsts.ChampionshipFlagImage, content, Path.GetExtension(upload.FileName), cancellationToken);
    }
}
