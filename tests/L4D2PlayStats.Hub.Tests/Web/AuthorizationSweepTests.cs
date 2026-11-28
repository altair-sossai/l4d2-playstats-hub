using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class AuthorizationSweepTests
{
    private const long Stranger = 76561198000000777;
    private static readonly string[] Unsafe = ["POST", "PUT", "DELETE", "PATCH"];

    [TestMethod]
    public void TheSweepSeesEveryKindOfEndpoint()
    {
        using var factory = new HubWebFactory();

        var routes = Routes(factory).ToList();

        Assert.IsGreaterThan(100, routes.Count(entry => entry.Route.Admin));
        Assert.IsGreaterThan(20, routes.Count(entry => entry.Route.GameServer));
        Assert.IsGreaterThan(15, routes.Count(entry => entry.Route.SignedIn && !entry.Route.Admin));
        Assert.IsTrue(routes.Any(entry => entry.Route.Pattern == "admin/bans/save" && entry.Route.Admin && entry.Route.Antiforgery));
        Assert.IsTrue(routes.Any(entry => entry.Route.Pattern == "api/bans/lift" && entry.Route.GameServer));
    }

    [TestMethod]
    public async Task NobodyButAnAdministratorReachesAnAdministrationEndpoint()
    {
        using var factory = await CreateAsync();
        using var anonymous = factory.Client();
        using var player = factory.Client(Stranger);

        var token = await TokenAsync(player);
        var problems = new List<string>();

        foreach (var (route, method) in Routes(factory).Where(entry => entry.Route.Admin))
        {
            var url = Url(route.Pattern);
            var asAnonymous = await SendAsync(anonymous, method, url, null);
            var asPlayer = await SendAsync(player, method, url, token);

            if (asAnonymous != HttpStatusCode.Unauthorized)
                problems.Add($"{method} {url} answered {(int)asAnonymous} to a visitor");

            if (asPlayer != HttpStatusCode.Forbidden)
                problems.Add($"{method} {url} answered {(int)asPlayer} to a player who is not an administrator");
        }

        Assert.IsEmpty(problems, string.Join("\n", problems));
    }

    [TestMethod]
    public async Task OnlyAGameServerReachesTheEndpointsOfThePlugins()
    {
        using var factory = await CreateAsync();
        using var anonymous = factory.Client();
        using var player = factory.Client(HubWebFactory.Administrator);
        using var wrongToken = factory.Client();

        var credentials = await factory.CreateServerAsync();
        var token = await TokenAsync(player);

        wrongToken.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{credentials.Server.PublicId}:wrong-key");

        var problems = new List<string>();

        foreach (var (route, method) in Routes(factory).Where(entry => entry.Route.GameServer))
        {
            var url = Url(route.Pattern);

            foreach (var (label, client, antiforgery) in new[] { ("a visitor", anonymous, (string?)null), ("an administrator signed in on the site", player, token), ("a wrong key", wrongToken, null) })
            {
                var status = await SendAsync(client, method, url, antiforgery);

                if (status != HttpStatusCode.Unauthorized)
                    problems.Add($"{method} {url} answered {(int)status} to {label}");
            }
        }

        Assert.IsEmpty(problems, string.Join("\n", problems));
    }

    [TestMethod]
    public async Task EveryEndpointForSignedInPlayersRefusesVisitors()
    {
        using var factory = await CreateAsync();
        using var anonymous = factory.Client();

        var problems = new List<string>();

        foreach (var (route, method) in Routes(factory).Where(entry => entry.Route.SignedIn && !entry.Route.Admin))
        {
            var url = Url(route.Pattern);
            var status = await SendAsync(anonymous, method, url, null);

            if (status != HttpStatusCode.Unauthorized)
                problems.Add($"{method} {url} answered {(int)status} to a visitor");
        }

        Assert.IsEmpty(problems, string.Join("\n", problems));
    }

    [TestMethod]
    public async Task EveryChangeMadeFromTheBrowserNeedsTheAntiforgeryToken()
    {
        using var factory = await CreateAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var problems = new List<string>();

        foreach (var (route, method) in Routes(factory).Where(entry => !entry.Route.GameServer))
        {
            if (!Unsafe.Contains(method))
                continue;

            var url = Url(route.Pattern);
            var status = await SendAsync(administrator, method, url, null);

            if (status != HttpStatusCode.BadRequest)
                problems.Add($"{method} {url} answered {(int)status} without the antiforgery token");
        }

        Assert.IsEmpty(problems, string.Join("\n", problems));
    }

    [TestMethod]
    public void EveryAdministrationAddressAndEveryChangeIsProtected()
    {
        using var factory = new HubWebFactory();

        var problems = new List<string>();

        foreach (var (route, method) in Routes(factory))
        {
            var pattern = route.Pattern;

            if ((pattern.StartsWith("admin", StringComparison.OrdinalIgnoreCase) || pattern.StartsWith("api/admin", StringComparison.OrdinalIgnoreCase)) && !route.Admin)
                problems.Add($"{method} /{pattern} is under the administration but has no administrator policy");

            if (Unsafe.Contains(method) && !route.GameServer && !route.Antiforgery)
                problems.Add($"{method} /{pattern} changes data without antiforgery validation");

            if (Unsafe.Contains(method) && !route.GameServer && !route.SignedIn && !PublicChanges.Contains($"{method} /{pattern}"))
                problems.Add($"{method} /{pattern} changes data and accepts visitors who are not signed in");
        }

        Assert.IsEmpty(problems, string.Join("\n", problems));
    }

    private static readonly HashSet<string> PublicChanges =
    [
        "POST /setup/start",
        "POST /setup/site",
        "POST /setup/modules",
        "POST /setup/server",
        "POST /setup/finish",
        "POST /logout",
        "POST /community/vote",
        "POST /servers/{publicId}/chat"
    ];

    private static async Task<HubWebFactory> CreateAsync()
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, SkinShop = true, CommunityRanking = true, SiteChat = true, RemoteCommands = true, AntiCheat = true, Championships = true, Bans = true });

        return factory;
    }

    private static async Task<string> TokenAsync(HttpClient client)
    {
        return HubWebFactory.Token(await client.GetStringAsync(WebConsts.NotificationsUrl));
    }

    private static IEnumerable<(RouteInfo Route, string Method)> Routes(HubWebFactory factory)
    {
        var sources = factory.Services.GetRequiredService<EndpointDataSource>();

        foreach (var endpoint in sources.Endpoints.OfType<RouteEndpoint>())
        {
            var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods;

            if (methods == null || methods.Count == 0 || endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>() == null)
                continue;

            var authorize = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            var anonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null;
            var route = new RouteInfo(
                endpoint.RoutePattern.RawText!.TrimStart('/'),
                !anonymous && authorize.Any(data => data.Policy == WebConsts.AdministratorPolicy),
                !anonymous && authorize.Count > 0,
                endpoint.Metadata.OfType<GameServerAuthorizeAttribute>().Any(),
                endpoint.Metadata.OfType<IAntiforgeryMetadata>().Any(data => data.RequiresValidation) || endpoint.Metadata.Any(data => data.GetType().Name is "AutoValidateAntiforgeryTokenAttribute" or "ValidateAntiForgeryTokenAttribute"));

            foreach (var method in methods)
                yield return (route, method);
        }
    }

    private static string Url(string pattern)
    {
        return "/" + Parameter().Replace(pattern, match =>
        {
            var name = match.Groups[1].Value;

            if (match.Value.Contains(":long", StringComparison.Ordinal) || name.Equals("communityId", StringComparison.OrdinalIgnoreCase) || name.Equals("player", StringComparison.OrdinalIgnoreCase))
                return HubWebFactory.Player.ToString();

            return match.Value.Contains(":int", StringComparison.Ordinal) || name.EndsWith("Id", StringComparison.Ordinal) || name == "id" ? "1" : "x";
        });
    }

    private static async Task<HttpStatusCode> SendAsync(HttpClient client, string method, string url, string? token)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (Unsafe.Contains(method))
        {
            var fields = new Dictionary<string, string>();

            if (token != null)
            {
                fields[HubWebFactory.TokenField] = token;
                request.Headers.Add(WebConsts.AntiforgeryHeader, token);
            }

            request.Content = method == "POST" ? new FormUrlEncodedContent(fields) : new StringContent("{}", Encoding.UTF8, "application/json");
        }

        using var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    [GeneratedRegex(@"\{\*{0,2}([A-Za-z]+)[^}]*\}")]
    private static partial Regex Parameter();

    private sealed record RouteInfo(string Pattern, bool Admin, bool SignedIn, bool GameServer, bool Antiforgery);
}
