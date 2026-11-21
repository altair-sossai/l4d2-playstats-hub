using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Extensions;

public static class DisplayNameStoreExtensions
{
    public static string? Find(this IDisplayNameStore store, string? communityId)
    {
        return long.TryParse(communityId, out var parsed) ? store.Find(parsed) : null;
    }

    public static string? Apply(this IDisplayNameStore store, string? communityId, string? name)
    {
        return store.Find(communityId) ?? name;
    }

    public static string? Apply(this IDisplayNameStore store, long? communityId, string? name)
    {
        return communityId == null ? name : store.Find(communityId.Value) ?? name;
    }
}
