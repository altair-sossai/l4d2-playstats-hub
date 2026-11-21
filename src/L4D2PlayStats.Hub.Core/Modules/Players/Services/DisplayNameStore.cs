using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class DisplayNameStore : IDisplayNameStore
{
    private ConcurrentDictionary<long, string> _names = new();

    public string? Find(long communityId)
    {
        return Volatile.Read(ref _names).GetValueOrDefault(communityId);
    }

    [return: NotNullIfNotNull(nameof(name))]
    public string? Apply(long communityId, string? name)
    {
        return Find(communityId) ?? name;
    }

    public void Set(long communityId, string? displayName)
    {
        var names = Volatile.Read(ref _names);
        var known = PlayerDisplayNames.Known(communityId, displayName);

        if (known == null)
            names.TryRemove(communityId, out _);
        else
            names[communityId] = known;
    }

    public void Replace(IEnumerable<KeyValuePair<long, string?>> names)
    {
        var loaded = new ConcurrentDictionary<long, string>();

        foreach (var (communityId, displayName) in names)
        {
            var known = PlayerDisplayNames.Known(communityId, displayName);

            if (known != null)
                loaded[communityId] = known;
        }

        Volatile.Write(ref _names, loaded);
    }
}
