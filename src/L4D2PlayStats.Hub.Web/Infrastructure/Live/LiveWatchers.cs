using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Live;

public class LiveWatchers
{
    private readonly ConcurrentDictionary<string, GameServer> _connections = new();

    public IReadOnlyList<GameServer> Servers => [.. _connections.Values.DistinctBy(server => server.Id)];

    public int Count => _connections.Count;

    public static string Group(string publicId)
    {
        return $"{AppConsts.LiveHubGroupPrefix}{publicId}";
    }

    public void Add(string connectionId, GameServer server)
    {
        _connections[connectionId] = server;
    }

    public GameServer? Remove(string connectionId)
    {
        return _connections.TryRemove(connectionId, out var server) ? server : null;
    }
}
