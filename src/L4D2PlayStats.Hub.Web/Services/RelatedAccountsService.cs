using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class RelatedAccountsService(
    IPlayerConnectionService playerConnectionService,
    ICommunityModerationService moderationService,
    IChatBanService chatBanService,
    IForcedNameStore forcedNames,
    IAntiCheatAccessService antiCheatAccessService,
    IBanService banService,
    ISiteClock siteClock) : IRelatedAccountsService
{
    public async Task<RelatedAccountsResult?> FindAsync(string? player, string? list, CancellationToken cancellationToken = default)
    {
        var communityId = SteamIdParser.Parse(player);
        if (communityId == null)
            return null;

        var related = (await playerConnectionService.RelatedAsync(communityId.Value, cancellationToken)).Take(AppConsts.RelatedAccountsMax).ToList();
        var ids = related.Select(account => long.Parse(account.CommunityId)).ToList();
        var linked = ids.Count == 0 ? [] : await LinkedAsync(list, ids, cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);

        return new RelatedAccountsResult(communityId.Value.ToString(), [.. related.Select(account => new RelatedAccountResult(account, linked.Contains(long.Parse(account.CommunityId)), timeZone))]);
    }

    public async Task<IReadOnlyList<long>> ValidAsync(long communityId, IEnumerable<string>? relatedIds, CancellationToken cancellationToken = default)
    {
        var requested = (relatedIds ?? [])
            .Select(SteamIdParser.Parse)
            .OfType<long>()
            .Where(id => id != communityId)
            .ToHashSet();

        if (requested.Count == 0)
            return [];

        var related = await playerConnectionService.RelatedAsync(communityId, cancellationToken);

        return [.. related.Select(account => long.Parse(account.CommunityId)).Where(requested.Contains).Take(AppConsts.RelatedAccountsMax)];
    }

    private async Task<HashSet<long>> LinkedAsync(string? list, List<long> ids, CancellationToken cancellationToken)
    {
        switch (list?.Trim().ToLowerInvariant())
        {
            case AppConsts.RelatedVoterBlocksList:
                var blocked = new HashSet<long>();

                foreach (var id in ids)
                    if (await moderationService.IsBlockedAsync(id, cancellationToken))
                        blocked.Add(id);

                return blocked;
            case AppConsts.RelatedChatBansList:
                return await chatBanService.BannedAsync(ids, cancellationToken);
            case AppConsts.RelatedBansList:
                return await banService.BannedAsync(ids, cancellationToken);
            case AppConsts.RelatedForcedNamesList:
                return [.. ids.Where(id => forcedNames.Find(id) != null)];
            case { } name when name.StartsWith(AppConsts.RelatedAntiCheatListPrefix, StringComparison.Ordinal) && AntiCheatAccessNames.TryParseList(name[AppConsts.RelatedAntiCheatListPrefix.Length..], out var accessList):
                return [.. await antiCheatAccessService.IdsAsync(accessList, cancellationToken)];
            default:
                return [];
        }
    }
}
