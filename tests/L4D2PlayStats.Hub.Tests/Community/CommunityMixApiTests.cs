using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityMixApiTests
{
    private const string Api = "/api/community/mix";
    private static readonly long[] Players = [.. Enumerable.Range(1, 8).Select(index => 76561198000000000L + index)];

    [TestMethod]
    public async Task TheRankedPlayersAreSplitInTwoTeams()
    {
        using var factory = new HubWebFactory();
        using var server = await StartAsync(factory, Players);

        var response = await server.PostAsJsonAsync(Api, new { players = Players.Select(player => player.ToString()) });
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        var survivors = body.GetProperty("survivors").EnumerateArray().Select(item => long.Parse(item.GetString()!)).ToList();
        var infected = body.GetProperty("infected").EnumerateArray().Select(item => long.Parse(item.GetString()!)).ToList();
        var chance = body.GetProperty("survivorsChance").GetDouble();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.HasCount(4, survivors);
        Assert.HasCount(4, infected);
        CollectionAssert.AreEquivalent(Players, survivors.Concat(infected).ToList());
        Assert.IsGreaterThan(0.4, chance);
        Assert.IsLessThan(0.6, chance);
    }

    [TestMethod]
    public async Task APlayerOutsideTheRankingStopsTheMixAndIsNamed()
    {
        using var factory = new HubWebFactory();
        using var server = await StartAsync(factory, Players[..7]);

        var response = await server.PostAsJsonAsync(Api, new { players = Players.Select(player => player.ToString()) });
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        Assert.AreEqual("unranked", body.GetProperty("error").GetString());
        CollectionAssert.AreEqual(new[] { Players[7].ToString() }, body.GetProperty("players").EnumerateArray().Select(item => item.GetString()).ToList());
    }

    [TestMethod]
    public async Task APlayerWithTooFewComparisonsIsOutsideTheRanking()
    {
        using var factory = new HubWebFactory();
        using var server = await StartAsync(factory, Players, Players[0]);

        var response = await server.PostAsJsonAsync(Api, new { players = Players.Select(player => player.ToString()) });
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        CollectionAssert.AreEqual(new[] { Players[0].ToString() }, body.GetProperty("players").EnumerateArray().Select(item => item.GetString()).ToList());
    }

    [TestMethod]
    [DataRow(7)]
    [DataRow(1)]
    [DataRow(0)]
    public async Task AnOddOrTooSmallListIsRejected(int count)
    {
        using var factory = new HubWebFactory();
        using var server = await StartAsync(factory, Players);

        var response = await server.PostAsJsonAsync(Api, new { players = Players.Take(count).Select(player => player.ToString()) });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task RepeatedOrInvalidPlayersAreRejected()
    {
        using var factory = new HubWebFactory();
        using var server = await StartAsync(factory, Players);

        var repeated = await server.PostAsJsonAsync(Api, new { players = Players[..6].Concat([Players[0], Players[1]]).Select(player => player.ToString()) });
        var invalid = await server.PostAsJsonAsync(Api, new { players = Players[..7].Select(player => player.ToString()).Append("abc") });

        Assert.AreEqual(HttpStatusCode.BadRequest, repeated.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [TestMethod]
    public async Task OnlyAGameServerCanAskForAMix()
    {
        using var factory = new HubWebFactory();
        using var server = await StartAsync(factory, Players);
        using var visitor = factory.Client();

        var response = await visitor.PostAsJsonAsync(Api, new { players = Players.Select(player => player.ToString()) });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task TheMixIsNotFoundWhileTheCommunityRankingIsOff()
    {
        using var factory = new HubWebFactory();
        using var server = await StartAsync(factory, Players);

        await factory.SaveAsync(new ModuleSettings { Ranking = true, CommunityRanking = false });

        var response = await server.PostAsJsonAsync(Api, new { players = Players.Select(player => player.ToString()) });

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<HttpClient> StartAsync(HubWebFactory factory, long[] ranked, long? fewComparisons = null)
    {
        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, CommunityRanking = true });

        var credentials = await factory.CreateServerAsync();

        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            foreach (var (player, index) in ranked.Select((player, index) => (player, index)))
                dbContext.Add(new CommunityRating { CommunityId = player, Rating = 1300 + index * 50, Deviation = 80, Comparisons = player == fewComparisons ? 1 : 20 });

            await dbContext.SaveChangesAsync();
        });

        return factory.ServerClient(credentials);
    }
}
