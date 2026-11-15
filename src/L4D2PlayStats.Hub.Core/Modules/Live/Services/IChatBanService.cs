namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public interface IChatBanService
{
    Task<ChatBan?> FindAsync(long communityId, CancellationToken cancellationToken = default);
    Task<List<ChatBan>> ListAsync(CancellationToken cancellationToken = default);
    Task<ChatBan?> SaveAsync(long communityId, string? name, string? reason, long? createdBy, string? createdByName, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(long communityId, CancellationToken cancellationToken = default);
}
