using System.Diagnostics.CodeAnalysis;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IDisplayNameStore
{
    string? Find(long communityId);
    [return: NotNullIfNotNull(nameof(name))]
    string? Apply(long communityId, string? name);
    void Set(long communityId, string? displayName);
    void Replace(IEnumerable<KeyValuePair<long, string?>> names);
}
