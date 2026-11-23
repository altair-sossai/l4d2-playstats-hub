using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    public async Task AnAdministratorReplacesAPlayerOfARoundHalfAndTheMatchFollows()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var token = await TokenAsync(administrator, match.Id, files[0].Id);

        var response = await PostAsync(administrator, files[0].Id, new { half = 0, survivor = true, replaced = Left.ToString(), replacement = Substitute.ToString() }, token);
        var answer = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        var after = await MatchAsync(factory);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(after.Id, answer.GetProperty("matchId").GetInt32());
        Assert.AreEqual(files[0].Id, answer.GetProperty("fileId").GetInt32());
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
        var token = await TokenAsync(administrator, match.Id, files[0].Id);

        var response = await PostAsync(administrator, files[0].Id, new { half = 0, survivor = true, replaced = Left.ToString(), replacement = "https://steamcommunity.com/profiles/76561198000000777" }, token);

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
        var token = await TokenAsync(administrator, match.Id, files[0].Id);
        var command = new { half = 0, survivor = true, replaced = Left.ToString(), replacement = Substitute.ToString() };

        var anonymous = await PostAsync(visitor, files[0].Id, command, token);
        var forbidden = await PostAsync(player, files[0].Id, command, token);
        var withoutToken = await PostAsync(administrator, files[0].Id, command, null);

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
        var token = await TokenAsync(administrator, match.Id, files[0].Id);
        var rival = StatisticsBuilder.Sample().Survivors('B')[0];

        var otherTeam = await PostAsync(administrator, files[0].Id, new { half = 0, survivor = true, replaced = Left.ToString(), replacement = rival.ToString() }, token);
        var wrongSide = await PostAsync(administrator, files[0].Id, new { half = 0, survivor = false, replaced = Left.ToString(), replacement = Substitute.ToString() }, token);
        var noReplacement = await PostAsync(administrator, files[0].Id, new { half = 0, survivor = true, replaced = Left.ToString(), replacement = "" }, token);
        var noHalf = await PostAsync(administrator, files[0].Id, new { survivor = true, replaced = Left.ToString(), replacement = Substitute.ToString() }, token);

        Assert.AreEqual(HttpStatusCode.BadRequest, otherTeam.StatusCode);
        Assert.AreEqual("The chosen player played this map for the other team.", await ErrorAsync(otherTeam));
        Assert.AreEqual("The player is no longer in this round half. Reload the page and try again.", await ErrorAsync(wrongSide));
        Assert.AreEqual("Choose the new player.", await ErrorAsync(noReplacement));
        Assert.AreEqual("Choose the round half and the side of the player.", await ErrorAsync(noHalf));
    }

    [TestMethod]
    public async Task TheAdministratorGetsAPencilNextToEveryPlayerOfTheMap()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var body = await administrator.WithScriptsAsync($"/matches/{match.Id}/statistics/{files[0].Id}");
        var values = PageScripts.Config(body, "matches-statistics").GetProperty("values");
        var pencils = Regex.Matches(body, "class=\"btn btn-link btn-xs ms-replace\" data-half=\"(\\d)\" data-survivor=\"(true|false)\" data-player=\"(\\d+)\"");

        Assert.HasCount(16, pencils);
        Assert.IsTrue(pencils.Any(pencil => pencil.Groups[1].Value == "0" && pencil.Groups[2].Value == "true" && pencil.Groups[3].Value == Left.ToString()));
        Assert.IsTrue(pencils.Any(pencil => pencil.Groups[1].Value == "1" && pencil.Groups[2].Value == "false" && pencil.Groups[3].Value == Substitute.ToString()));
        Assert.AreEqual($"/api/statistics/{files[0].Id}/replace-player", values.GetProperty("replacePlayerUrl").GetString());
        Assert.Contains("id=\"pp-modal\"", body);
        Assert.Contains("PlayerPicker.open(", body);
    }

    [TestMethod]
    public async Task AVisitorDoesNotGetThePencils()
    {
        using var factory = await FactoryWithSubstitutionAsync();
        using var visitor = factory.Client();

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var body = await visitor.GetStringAsync($"/matches/{match.Id}/statistics/{files[0].Id}");

        Assert.DoesNotContain("ms-replace", body);
        Assert.DoesNotContain("matches-statistics-config", body);
        Assert.DoesNotContain("pp-modal", body);
    }

    // As in match 1873 of l4d2.com.br: the player who left was a survivor in the first half of the first map, and the
    // substitute came in as an infected of the same team in the second half and played the second map.
    private static async Task<HubWebFactory> FactoryWithSubstitutionAsync()
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = DateTime.UtcNow;
        var night = new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200).WithSubstitute(Left, Substitute, "hard", fromHalf: 1));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(15), 2, 900, 450).WithSubstitute(Left, Substitute, "hard"));

        return factory;
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, int fileId, object command, string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/statistics/{fileId}/replace-player") { Content = JsonContent.Create(command) };

        if (token != null)
            request.Headers.Add(TokenHeader, token);

        return client.SendAsync(request);
    }

    private static async Task<string?> ErrorAsync(HttpResponseMessage response)
    {
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("errorMessage").GetString();
    }

    private static async Task<string> TokenAsync(HttpClient administrator, int matchId, int fileId)
    {
        var body = await administrator.GetStringAsync($"/matches/{matchId}/statistics/{fileId}");
        var values = PageScripts.Config(body, "matches-statistics").GetProperty("values");

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
