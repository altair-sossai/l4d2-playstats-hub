using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class ChatBanService(HubDbContext dbContext, IClock clock, IPlayerNameService playerNameService) : IChatBanService
{
    public Task<ChatBan?> FindAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatBan>().FirstOrDefaultAsync(ban => ban.CommunityId == communityId, cancellationToken);
    }

    public Task<List<ChatBan>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatBan>().AsNoTracking().OrderByDescending(ban => ban.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task<Page<ChatBan>> PageAsync(PlayerSearch search, int page, int size, CancellationToken cancellationToken = default)
    {
        return search
            .Apply(dbContext.Set<ChatBan>().AsNoTracking(), ban => ban.CommunityId, ban => ban.Name, ban => ban.Reason)
            .OrderByDescending(ban => ban.CreatedAtUtc)
            .ThenBy(ban => ban.CommunityId)
            .ToPageAsync(page, size, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatBan>().CountAsync(cancellationToken);
    }

    public async Task<HashSet<long>> BannedAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        List<long> ids = [.. communityIds.Distinct()];

        var banned = await dbContext.Set<ChatBan>()
            .AsNoTracking()
            .Where(ban => ids.Contains(ban.CommunityId))
            .Select(ban => ban.CommunityId)
            .ToListAsync(cancellationToken);

        return [.. banned];
    }

    public async Task<ChatBan?> SaveAsync(long communityId, string? name, string? reason, long? createdBy, string? createdByName, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(communityId))
            return null;

        var ban = await FindAsync(communityId, cancellationToken);
        var now = clock.UtcNow;

        if (ban == null)
        {
            ban = new ChatBan
            {
                CommunityId = communityId,
                CreatedByCommunityId = createdBy,
                CreatedByName = Text.Clip(createdByName, AppConsts.LivePlayerNameMaxLength),
                CreatedAtUtc = now
            };

            dbContext.Set<ChatBan>().Add(ban);
        }
        else
            ban.UpdatedAtUtc = now;

        if (!string.IsNullOrWhiteSpace(name))
            ban.Name = Text.Clip(name, AppConsts.LivePlayerNameMaxLength);

        if (string.IsNullOrWhiteSpace(ban.Name))
            ban.Name = Text.Clip(await playerNameService.RealNameAsync(communityId, cancellationToken), AppConsts.LivePlayerNameMaxLength);

        ban.Reason = Text.Clip(reason, AppConsts.ChatBanReasonMaxLength);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ban;
    }

    public async Task<bool> RemoveAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var ban = await FindAsync(communityId, cancellationToken);
        if (ban == null)
            return false;

        dbContext.Set<ChatBan>().Remove(ban);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
