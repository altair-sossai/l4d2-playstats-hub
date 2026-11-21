namespace L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

public interface ICommunityRatingService
{
    Task<int> RecalculateAsync(long? withoutVoterId = null, CancellationToken cancellationToken = default);
    Task<bool> NeedsRecalculationAsync(CancellationToken cancellationToken = default);
}
