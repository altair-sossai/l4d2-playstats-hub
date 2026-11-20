using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Steam;

public partial class SteamProfileClient(IHttpClientFactory httpClientFactory, IClock clock)
{
    private DateTime _pausedUntilUtc = DateTime.MinValue;

    public bool Paused => clock.UtcNow < _pausedUntilUtc;

    public HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient(AppConsts.SteamHttpClient);
        client.Timeout = TimeSpan.FromSeconds(AppConsts.SteamTimeoutInSeconds);

        return client;
    }

    public async Task<SteamProfileLookup> LookupAsync(IReadOnlyCollection<long> ids, string? key, CancellationToken cancellationToken)
    {
        var profiles = new Dictionary<long, SteamProfile>();
        var answered = new HashSet<long>();

        if (ids.Count == 0 || Paused)
            return new SteamProfileLookup(profiles, answered);

        var client = CreateClient();

        if (string.IsNullOrWhiteSpace(key))
            await KeylessAsync(client, ids, profiles, answered, cancellationToken);
        else
            await SummariesAsync(client, key.Trim(), ids, profiles, answered, cancellationToken);

        return new SteamProfileLookup(profiles, answered);
    }

    private static async Task SummariesAsync(HttpClient client, string key, IReadOnlyCollection<long> ids, Dictionary<long, SteamProfile> profiles, HashSet<long> answered, CancellationToken cancellationToken)
    {
        foreach (var batch in ids.Chunk(AppConsts.AvatarBatchSize))
        {
            var url = $"{AppConsts.SteamSummariesUrl}?key={Uri.EscapeDataString(key)}&steamids={string.Join(",", batch)}";

            try
            {
                using var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return;

                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

                answered.UnionWith(batch);

                if (!document.RootElement.TryGetProperty("response", out var body) || !body.TryGetProperty("players", out var players))
                    continue;

                foreach (var player in players.EnumerateArray())
                {
                    if (!player.TryGetProperty("steamid", out var steamId) || !long.TryParse(steamId.GetString(), out var communityId))
                        continue;

                    var name = player.TryGetProperty("personaname", out var persona) ? persona.GetString() : null;
                    var avatar = player.TryGetProperty("avatarfull", out var full) ? full.GetString() : null;

                    profiles[communityId] = new SteamProfile(communityId, name, avatar);
                }
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException && !cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task KeylessAsync(HttpClient client, IReadOnlyCollection<long> ids, Dictionary<long, SteamProfile> profiles, HashSet<long> answered, CancellationToken cancellationToken)
    {
        foreach (var batch in ids.Chunk(AppConsts.AvatarKeylessParallel))
        {
            var results = await Task.WhenAll(batch.Select(id => ProfileAsync(client, id, cancellationToken)));

            foreach (var (communityId, profile, ok, limited) in results)
            {
                if (limited)
                {
                    _pausedUntilUtc = clock.UtcNow.AddMinutes(AppConsts.AvatarKeylessPauseInMinutes);

                    return;
                }

                if (!ok)
                    continue;

                answered.Add(communityId);

                if (profile != null)
                    profiles[communityId] = profile;
            }

            await Task.Delay(AppConsts.AvatarKeylessDelayInMilliseconds, cancellationToken);
        }
    }

    private static async Task<(long CommunityId, SteamProfile? Profile, bool Ok, bool Limited)> ProfileAsync(HttpClient client, long communityId, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.GetAsync(string.Format(AppConsts.SteamProfileXmlUrl, communityId), cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return (communityId, null, false, true);

            if (!response.IsSuccessStatusCode)
                return (communityId, null, false, false);

            var xml = await response.Content.ReadAsStringAsync(cancellationToken);
            var name = Value(NamePattern().Match(xml));
            var avatar = Value(AvatarPattern().Match(xml));

            return (communityId, name == null && avatar == null ? null : new SteamProfile(communityId, name, avatar), true, false);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return (communityId, null, false, false);
        }
    }

    private static string? Value(Match match)
    {
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex(AppConsts.SteamProfileNamePattern)]
    private static partial Regex NamePattern();

    [GeneratedRegex(AppConsts.SteamProfileAvatarPattern)]
    private static partial Regex AvatarPattern();
}
