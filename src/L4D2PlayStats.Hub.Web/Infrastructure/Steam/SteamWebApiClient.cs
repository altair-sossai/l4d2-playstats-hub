using System.Globalization;
using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Steam;

public class SteamWebApiClient(IHttpClientFactory httpClientFactory, IClock clock, ILogger<SteamWebApiClient> logger)
{
    private DateTime _pausedUntilUtc = DateTime.MinValue;

    public bool Paused => clock.UtcNow < _pausedUntilUtc;

    public async Task<SteamAnswer<IReadOnlyDictionary<long, SteamSummary>>> SummariesAsync(IReadOnlyCollection<long> ids, string key, CancellationToken cancellationToken)
    {
        var url = $"{AppConsts.SteamSummariesUrl}?key={Uri.EscapeDataString(key)}&steamids={string.Join(",", ids)}";
        using var document = await GetAsync(url, nameof(SummariesAsync), cancellationToken);

        if (document == null)
            return new SteamAnswer<IReadOnlyDictionary<long, SteamSummary>>(false, new Dictionary<long, SteamSummary>());

        var summaries = new Dictionary<long, SteamSummary>();

        if (document.RootElement.TryGetProperty("response", out var body) && body.TryGetProperty("players", out var players) && players.ValueKind == JsonValueKind.Array)
        {
            foreach (var player in players.EnumerateArray())
            {
                if (!TryId(player, "steamid", out var communityId))
                    continue;

                var name = player.TryGetProperty("personaname", out var persona) && persona.ValueKind == JsonValueKind.String ? persona.GetString() : null;
                var created = Number(player, "timecreated") is { } seconds and > 0 ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime : (DateTime?)null;

                summaries[communityId] = new SteamSummary(communityId, name, created);
            }
        }

        return new SteamAnswer<IReadOnlyDictionary<long, SteamSummary>>(true, summaries);
    }

    public async Task<SteamAnswer<IReadOnlyDictionary<long, SteamBans>>> BansAsync(IReadOnlyCollection<long> ids, string key, CancellationToken cancellationToken)
    {
        var url = $"{AppConsts.SteamBansUrl}?key={Uri.EscapeDataString(key)}&steamids={string.Join(",", ids)}";
        using var document = await GetAsync(url, nameof(BansAsync), cancellationToken);

        if (document == null)
            return new SteamAnswer<IReadOnlyDictionary<long, SteamBans>>(false, new Dictionary<long, SteamBans>());

        var bans = new Dictionary<long, SteamBans>();

        if (document.RootElement.TryGetProperty("players", out var players) && players.ValueKind == JsonValueKind.Array)
        {
            foreach (var player in players.EnumerateArray())
            {
                if (!TryId(player, "SteamId", out var communityId))
                    continue;

                bans[communityId] = new SteamBans(communityId, (int)(Number(player, "NumberOfVACBans") ?? 0), (int)(Number(player, "NumberOfGameBans") ?? 0), (int)(Number(player, "DaysSinceLastBan") ?? 0));
            }
        }

        return new SteamAnswer<IReadOnlyDictionary<long, SteamBans>>(true, bans);
    }

    public async Task<SteamAnswer<SteamPlaytime?>> PlaytimeAsync(long communityId, string key, CancellationToken cancellationToken)
    {
        var url = $"{AppConsts.SteamOwnedGamesUrl}?key={Uri.EscapeDataString(key)}&steamid={communityId}&include_played_free_games=1&appids_filter[0]={AppConsts.Left4Dead2AppId}";
        using var document = await GetAsync(url, nameof(PlaytimeAsync), cancellationToken);

        if (document == null)
            return new SteamAnswer<SteamPlaytime?>(false, null);

        if (!document.RootElement.TryGetProperty("response", out var body) || !body.TryGetProperty("games", out var games) || games.ValueKind != JsonValueKind.Array)
            return new SteamAnswer<SteamPlaytime?>(true, null);

        foreach (var game in games.EnumerateArray())
        {
            if (Number(game, "appid") != AppConsts.Left4Dead2AppId)
                continue;

            return new SteamAnswer<SteamPlaytime?>(true, new SteamPlaytime((int)(Number(game, "playtime_forever") ?? 0), (int)(Number(game, "playtime_2weeks") ?? 0)));
        }

        return new SteamAnswer<SteamPlaytime?>(true, null);
    }

    public async Task<SteamAnswer<int?>> LevelAsync(long communityId, string key, CancellationToken cancellationToken)
    {
        var url = $"{AppConsts.SteamLevelUrl}?key={Uri.EscapeDataString(key)}&steamid={communityId}";
        using var document = await GetAsync(url, nameof(LevelAsync), cancellationToken);

        if (document == null)
            return new SteamAnswer<int?>(false, null);

        var level = document.RootElement.TryGetProperty("response", out var body) ? Number(body, "player_level") : null;

        return new SteamAnswer<int?>(true, level == null ? null : (int)level.Value);
    }

    private async Task<JsonDocument?> GetAsync(string url, string call, CancellationToken cancellationToken)
    {
        if (Paused)
            return null;

        try
        {
            var client = httpClientFactory.CreateClient(AppConsts.SteamHttpClient);
            client.Timeout = TimeSpan.FromSeconds(AppConsts.SteamTimeoutInSeconds);

            using var response = await client.GetAsync(url, cancellationToken);

            if (response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                _pausedUntilUtc = clock.UtcNow.AddMinutes(AppConsts.SteamDataPauseInMinutes);
                logger.LogWarning("The Steam Web API answered {Status} to {Call}. The Steam data is paused for {Minutes} minutes.", (int)response.StatusCode, call, AppConsts.SteamDataPauseInMinutes);

                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("The Steam Web API answered {Status} to {Call}. It will be tried again later.", (int)response.StatusCode, call);

                return null;
            }

            return JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("The Steam Web API call {Call} failed: {Message}. It will be tried again later.", call, exception.Message);

            return null;
        }
    }

    private static bool TryId(JsonElement element, string property, out long communityId)
    {
        communityId = 0;

        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), NumberStyles.None, CultureInfo.InvariantCulture, out communityId);
    }

    private static long? Number(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number) ? number : null;
    }
}
