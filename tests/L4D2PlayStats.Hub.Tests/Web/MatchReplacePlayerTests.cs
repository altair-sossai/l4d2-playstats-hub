using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Match = L4D2PlayStats.Hub.Core.Modules.Matches.Match;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class MatchReplacePlayerTests
{
    private const string TokenHeader = "RequestVerificationToken";
    private const long Left = StatisticsBuilder.PlayerA1;
    private const long Substitute = 76561198000000777;

    [TestMethod]
    public async Task AnAdministratorReplacesThePlayerWhoLeftAndTheMatchFollows()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var token = await TokenAsync(administrator, match.Id);

        var response = await PostAsync(administrator, match.Id, new { replaced = Left.ToString(), replacement = Substitute.ToString(), fileIds = new[] { files[0].Id } }, token);
        var answer = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        var after = await MatchAsync(factory);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(after.Id, answer.GetProperty("matchId").GetInt32());
        Assert.IsTrue(after.Players.Single(p => p.CommunityId == Substitute).Win);
        Assert.IsFalse(after.Players.Any(p => p.CommunityId == Left));
    }

    [TestMethod]
    public async Task AnyFormOfSteamIdIsAccepted()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var token = await TokenAsync(administrator, match.Id);

        var response = await PostAsync(administrator, match.Id, new { replaced = Left.ToString(), replacement = "https://steamcommunity.com/profiles/76561198000000777", fileIds = new[] { files[0].Id } }, token);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue((await MatchAsync(factory)).Players.Single(p => p.CommunityId == Substitute).Win);
    }

    [TestMethod]
    public async Task NobodyButAnAdministratorReplacesAPlayer()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var visitor = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var token = await TokenAsync(administrator, match.Id);
        var command = new { replaced = Left.ToString(), replacement = Substitute.ToString(), fileIds = new[] { files[0].Id } };

        var anonymous = await PostAsync(visitor, match.Id, command, token);
        var forbidden = await PostAsync(player, match.Id, command, token);
        var withoutToken = await PostAsync(administrator, match.Id, command, null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.AreEqual(files[0].Content, (await FilesAsync(factory))[0].Content);
    }

    [TestMethod]
    public async Task TheAdministratorIsToldWhyAReplacementWasRefused()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var token = await TokenAsync(administrator, match.Id);

        var notInMap = await PostAsync(administrator, match.Id, new { replaced = Left.ToString(), replacement = "76561198000000888", fileIds = new[] { files[1].Id } }, token);
        var noReplacement = await PostAsync(administrator, match.Id, new { replaced = Left.ToString(), replacement = "", fileIds = new[] { files[0].Id } }, token);
        var noMaps = await PostAsync(administrator, match.Id, new { replaced = Left.ToString(), replacement = Substitute.ToString(), fileIds = Array.Empty<int>() }, token);

        Assert.AreEqual(HttpStatusCode.BadRequest, notInMap.StatusCode);
        Assert.AreEqual("The player who left did not play the map c2m2_fairgrounds.", await ErrorAsync(notInMap));
        Assert.AreEqual("Choose the player who came in.", await ErrorAsync(noReplacement));
        Assert.AreEqual("Choose at least one map.", await ErrorAsync(noMaps));
    }

    [TestMethod]
    public async Task TheAdministratorGetsTheWindowWithEveryoneWhoPlayedAMap()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var body = await administrator.WithScriptsAsync($"/matches/{match.Id}");
        var values = PageScripts.Config(body, "matches-details").GetProperty("values");
        var maps = values.GetProperty("maps").EnumerateArray().ToList();
        var players = values.GetProperty("players").EnumerateArray().Select(p => p.GetProperty("communityId").GetString()).ToList();

        Assert.Contains("id=\"mrp-modal\"", body);
        Assert.Contains("id=\"pp-modal\"", body);
        Assert.Contains("PlayerPicker.swap($modal", body);
        Assert.AreEqual($"/api/matches/{match.Id}/replace-player", values.GetProperty("replacePlayerUrl").GetString());
        Assert.HasCount(2, maps);
        Assert.AreEqual(files[0].Id, maps[0].GetProperty("fileId").GetInt32());
        Assert.AreEqual("c2m1_highway", maps[0].GetProperty("mapName").GetString());
        Assert.Contains(Left.ToString(), maps[0].GetProperty("players").EnumerateArray().Select(p => p.GetString()).ToList());
        Assert.DoesNotContain(Left.ToString(), maps[1].GetProperty("players").EnumerateArray().Select(p => p.GetString()).ToList());
        Assert.Contains(Left.ToString(), players);
        Assert.Contains(Substitute.ToString(), players);
    }

    [TestMethod]
    public async Task AVisitorDoesNotGetTheWindow()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var visitor = factory.Client();

        var match = await MatchAsync(factory);
        var body = await visitor.GetStringAsync($"/matches/{match.Id}");

        Assert.DoesNotContain("mrp-modal", body);
        Assert.DoesNotContain("mrp-open", body);
        Assert.DoesNotContain("pp-modal", body);
    }

    private static async Task<HubWebFactory> FactoryWithSubstitutionAsync()
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = DateTime.UtcNow;
        var night = new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(15), 2, 900, 450).WithSubstitute(Left, Substitute, "Sub"));

        return factory;
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, int matchId, object command, string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/matches/{matchId}/replace-player") { Content = JsonContent.Create(command) };

        if (token != null)
            request.Headers.Add(TokenHeader, token);

        return client.SendAsync(request);
    }

    private static async Task<string?> ErrorAsync(HttpResponseMessage response)
    {
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("errorMessage").GetString();
    }

    private static async Task<string> TokenAsync(HttpClient administrator, int matchId)
    {
        var body = await administrator.GetStringAsync($"/matches/{matchId}");
        var values = PageScripts.Config(body, "matches-details").GetProperty("values");

        return values.GetProperty("antiforgeryHeaders").GetProperty(TokenHeader).GetString()!;
    }

    private static Task<Match> MatchAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().Include(m => m.Players).SingleAsync());
    }

    private static Task<List<StatisticsFile>> FilesAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.AsNoTracking().OrderBy(file => file.FileDate).ToListAsync());
    }
}
