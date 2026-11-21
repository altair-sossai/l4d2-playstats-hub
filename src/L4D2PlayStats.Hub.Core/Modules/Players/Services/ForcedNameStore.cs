using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class ForcedNameStore : IForcedNameStore
{
    private FrozenDictionary<long, string> _names = FrozenDictionary<long, string>.Empty;

    public IReadOnlyDictionary<long, string> All => Volatile.Read(ref _names);

    public string? Find(long communityId)
    {
        return Volatile.Read(ref _names).GetValueOrDefault(communityId);
    }

    [return: NotNullIfNotNull(nameof(name))]
    public string? Apply(long communityId, string? name)
    {
        return Find(communityId) ?? name;
    }

    public void Replace(IReadOnlyDictionary<long, string> names)
    {
        Volatile.Write(ref _names, names.ToFrozenDictionary());
    }
}
