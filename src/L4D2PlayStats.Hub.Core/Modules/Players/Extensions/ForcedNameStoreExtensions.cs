using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Extensions;

public static class ForcedNameStoreExtensions
{
    public static string? Find(this IForcedNameStore store, string? communityId)
    {
        return long.TryParse(communityId, out var parsed) ? store.Find(parsed) : null;
    }

    public static string? Apply(this IForcedNameStore store, string? communityId, string? name)
    {
        return store.Find(communityId) ?? name;
    }

    public static string? Apply(this IForcedNameStore store, long? communityId, string? name)
    {
        return communityId == null ? name : store.Find(communityId.Value) ?? name;
    }
}
