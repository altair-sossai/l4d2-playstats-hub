using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public class SkinAuthorService(HubDbContext dbContext, HubState hubState, IClock clock) : ISkinAuthorService
{
    public async Task<IReadOnlyList<long>> DueAsync(DateTime syncedBeforeUtc, int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            return [];

        var authors = await AuthorIdsAsync(cancellationToken);

        var syncedAt = await dbContext.Set<SkinAuthor>()
            .AsNoTracking()
            .ToDictionaryAsync(author => author.CommunityId, author => author.SyncedAtUtc, cancellationToken);

        return
        [
            .. authors
                .Select(id => (Id: id, SyncedAt: syncedAt.TryGetValue(id, out var value) ? value : DateTime.MinValue))
                .Where(author => author.SyncedAt < syncedBeforeUtc)
                .OrderBy(author => author.SyncedAt)
                .ThenBy(author => author.Id)
                .Select(author => author.Id)
                .Take(count)
        ];
    }

    public async Task<int> ApplyAsync(IReadOnlyCollection<long> checkedIds, IReadOnlyDictionary<long, string> names, CancellationToken cancellationToken = default)
    {
        if (checkedIds.Count == 0)
            return 0;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var now = clock.UtcNow;
            var ids = checkedIds.ToHashSet();
            var authors = await dbContext.Set<SkinAuthor>().Where(author => ids.Contains(author.CommunityId)).ToDictionaryAsync(author => author.CommunityId, cancellationToken);
            var skins = await dbContext.Set<Skin>().Where(skin => skin.AuthorCommunityId != null && ids.Contains(skin.AuthorCommunityId.Value)).ToListAsync(cancellationToken);
            var renamed = 0;

            foreach (var id in ids)
            {
                if (!authors.TryGetValue(id, out var author))
                {
                    author = new SkinAuthor { CommunityId = id };
                    dbContext.Add(author);
                }

                author.SyncedAtUtc = now;

                var name = Clean(names.GetValueOrDefault(id));
                if (name == null)
                    continue;

                if (author.Name != null && author.Name != name)
                    author.RenamedAtUtc = now;

                author.Name = name;

                foreach (var skin in skins.Where(skin => skin.AuthorCommunityId == id && skin.AuthorName != name))
                {
                    skin.AuthorName = name;
                    skin.UpdatedAtUtc = now;
                    renamed++;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return renamed;
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    public async Task<SkinAuthorStatus> StatusAsync(CancellationToken cancellationToken = default)
    {
        var authors = await AuthorIdsAsync(cancellationToken);

        var synced = await dbContext.Set<SkinAuthor>()
            .AsNoTracking()
            .Where(author => authors.Contains(author.CommunityId))
            .Select(author => new { author.SyncedAtUtc, author.RenamedAtUtc })
            .ToListAsync(cancellationToken);

        return new SkinAuthorStatus(
            authors.Count,
            synced.Count,
            synced.Count == 0 ? null : synced.Max(author => author.SyncedAtUtc),
            synced.Max(author => author.RenamedAtUtc));
    }

    private async Task<List<long>> AuthorIdsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Set<Skin>()
            .AsNoTracking()
            .Where(skin => skin.AuthorCommunityId != null)
            .Select(skin => skin.AuthorCommunityId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static string? Clean(string? name)
    {
        var trimmed = name?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            return null;

        return trimmed.Length > AppConsts.SkinNameMaxLength ? trimmed[..AppConsts.SkinNameMaxLength] : trimmed;
    }
}