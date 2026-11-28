using L4D2PlayStats.Hub.Web.Models.Championships;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IChampionshipImageService
{
    Task ValidateAsync(int championshipId, ChampionshipImagesUpload upload, CancellationToken cancellationToken = default);
    Task SaveAsync(int championshipId, ChampionshipImagesUpload upload, long? updatedBy, CancellationToken cancellationToken = default);
    void DeleteAll(int championshipId);
}
