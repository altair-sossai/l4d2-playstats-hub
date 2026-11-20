using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers.Enums;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace L4D2PlayStats.Hub.Core.Modules.Servers.Services;

public class GameServerService(HubDbContext dbContext, HubState hubState, IGameServerPresence presence, ILiveServerStore liveServerStore, IClock clock) : IGameServerService
{
    public Task<List<GameServer>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.GameServers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(cancellationToken);
    }

    public Task<GameServer?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.GameServers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<GameServer?> FindAsync(string publicId, CancellationToken cancellationToken = default)
    {
        return dbContext.GameServers.FirstOrDefaultAsync(s => s.PublicId == publicId, cancellationToken);
    }

    public async Task<GameServerCredentials> CreateAsync(string name, string? address, CancellationToken cancellationToken = default)
    {
        var key = SecretGenerator.NewSecret(AppConsts.GameServerKeyBytes);

        var server = new GameServer
        {
            PublicId = await NewPublicIdAsync(name, cancellationToken),
            Name = name.Trim(),
            Address = Normalize(address),
            KeyHash = SecretGenerator.Hash(key),
            CreatedAtUtc = clock.UtcNow
        };

        dbContext.GameServers.Add(server);
        await dbContext.SaveChangesAsync(cancellationToken);
        hubState.DataChanged();

        return new GameServerCredentials(server, key);
    }

    public async Task<GameServer?> UpdateAsync(int id, string name, string? address, bool enabled, CancellationToken cancellationToken = default)
    {
        var server = await FindAsync(id, cancellationToken);
        if (server == null)
            return null;

        server.Name = name.Trim();
        server.Address = Normalize(address);
        server.Enabled = enabled;

        await dbContext.SaveChangesAsync(cancellationToken);
        hubState.DataChanged();

        return server;
    }

    public async Task<GameServer?> SetAlternativeAddressAsync(int id, string? alternativeAddress, CancellationToken cancellationToken = default)
    {
        var server = await FindAsync(id, cancellationToken);
        if (server == null)
            return null;

        server.AlternativeAddress = Normalize(alternativeAddress);

        await dbContext.SaveChangesAsync(cancellationToken);

        return server;
    }

    public async Task<GameServerCredentials?> RegenerateKeyAsync(int id, CancellationToken cancellationToken = default)
    {
        var server = await FindAsync(id, cancellationToken);
        if (server == null)
            return null;

        var key = SecretGenerator.NewSecret(AppConsts.GameServerKeyBytes);
        server.KeyHash = SecretGenerator.Hash(key);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new GameServerCredentials(server, key);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var server = await FindAsync(id, cancellationToken);
        if (server == null)
            return false;

        if (await dbContext.StatisticsFiles.AnyAsync(f => f.GameServerId == id, cancellationToken))
            return false;

        dbContext.GameServers.Remove(server);
        await dbContext.SaveChangesAsync(cancellationToken);
        presence.Forget(server);
        liveServerStore.Forget(server.Id);
        hubState.DataChanged();

        return true;
    }

    public async Task<GameServerAuthentication> AuthenticateAsync(string? token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new GameServerAuthentication(GameServerAuthenticationStatus.MissingToken, null);

        var segments = token.Trim().Split(AppConsts.GameServerTokenSeparator, 2);
        if (segments.Length != 2 || string.IsNullOrEmpty(segments[0]) || string.IsNullOrEmpty(segments[1]))
            return new GameServerAuthentication(GameServerAuthenticationStatus.MissingToken, null);

        var publicId = segments[0];
        var server = await FindAsync(publicId, cancellationToken);
        if (server == null)
            return new GameServerAuthentication(GameServerAuthenticationStatus.UnknownServer, null);

        if (!SecretGenerator.Matches(segments[1], server.KeyHash))
        {
            presence.InvalidKey(server.PublicId);
            return new GameServerAuthentication(GameServerAuthenticationStatus.InvalidKey, null);
        }

        if (!server.Enabled)
            return new GameServerAuthentication(GameServerAuthenticationStatus.Disabled, server);

        presence.Seen(server.Id);

        return new GameServerAuthentication(GameServerAuthenticationStatus.Success, server);
    }

    public async Task SignalAsync(GameServer server, GameServerSignal signal, CancellationToken cancellationToken = default)
    {
        if (!presence.Signal(server.Id, signal))
            return;

        server.LastSeenAtUtc = clock.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> NewPublicIdAsync(string name, CancellationToken cancellationToken)
    {
        var slug = Slug(name);
        var candidate = slug;
        var suffix = 2;

        while (await dbContext.GameServers.AnyAsync(s => s.PublicId == candidate, cancellationToken))
        {
            candidate = $"{slug}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static string Slug(string name)
    {
        var builder = new StringBuilder();

        foreach (var character in name.Trim().ToLowerInvariant())
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
                builder.Append(character);
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length > AppConsts.GameServerPublicIdMaxLength)
            slug = slug[..AppConsts.GameServerPublicIdMaxLength].Trim('-');

        return string.IsNullOrEmpty(slug) ? "server" : slug;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
