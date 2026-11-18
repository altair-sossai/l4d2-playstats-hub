using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class ChatBanService(HubDbContext dbContext, IClock clock) : IChatBanService
{
    public Task<ChatBan?> FindAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatBan>().FirstOrDefaultAsync(ban => ban.CommunityId == communityId, cancellationToken);
    }

    public Task<List<ChatBan>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatBan>().AsNoTracking().OrderByDescending(ban => ban.CreatedAtUtc).ToListAsync(cancellationToken);
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
                CreatedByName = Limit(createdByName, AppConsts.LivePlayerNameMaxLength),
                CreatedAtUtc = now
            };

            dbContext.Set<ChatBan>().Add(ban);
        }
        else
        {
            ban.UpdatedAtUtc = now;
        }

        if (!string.IsNullOrWhiteSpace(name))
            ban.Name = Limit(name, AppConsts.LivePlayerNameMaxLength);

        if (string.IsNullOrWhiteSpace(ban.Name))
            ban.Name = Limit(await KnownNameAsync(communityId, cancellationToken), AppConsts.LivePlayerNameMaxLength);

        ban.Reason = Limit(reason, AppConsts.ChatBanReasonMaxLength);

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

    private async Task<string?> KnownNameAsync(long communityId, CancellationToken cancellationToken)
    {
        var profile = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(p => p.CommunityId == communityId && p.Name != null)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(profile))
            return profile;

        return await dbContext.Set<PlayerConnection>()
            .AsNoTracking()
            .Where(c => c.CommunityId == communityId)
            .OrderByDescending(c => c.LastSeenAtUtc)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string? Limit(string? value, int length)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var text = value.Trim();

        return text.Length > length ? text[..length] : text;
    }
}
