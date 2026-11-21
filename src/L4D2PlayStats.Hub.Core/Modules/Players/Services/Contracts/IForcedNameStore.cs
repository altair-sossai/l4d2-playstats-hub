using System.Diagnostics.CodeAnalysis;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IForcedNameStore
{
    IReadOnlyDictionary<long, string> All { get; }
    string? Find(long communityId);
    [return: NotNullIfNotNull(nameof(name))]
    string? Apply(long communityId, string? name);
    void Replace(IReadOnlyDictionary<long, string> names);
}
