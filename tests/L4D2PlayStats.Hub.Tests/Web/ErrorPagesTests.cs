using System.Net;
using System.Net.Http.Headers;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class ErrorPagesTests
{
    [TestMethod]
    public async Task APageThatThrowsShowsTheErrorPageInTheSiteLayoutAndLogsTheException()
    {
        using var factory = new HubWebFactory();
        var logs = new LogCollector();

        await factory.CompleteSetupAsync();

        using var broken = Broken(factory, logs);
        using var client = Client(broken);

        var response = await client.GetAsync("/matches?page=2");
        var html = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("<h2>Something went wrong</h2>", html);
        Assert.Contains("Error 500", html);
        Assert.Contains("/css/bootstrap.min.css", html);
        Assert.Contains("id=\"bs-navbar-collapse\"", html);
        Assert.Contains("<meta name=\"robots\" content=\"noindex", html);
        Assert.Contains("returnUrl=%2Fmatches%3Fpage%3D2", html);
        Assert.DoesNotContain(ThrowingProxy.Message, html);

        var errors = logs.Entries.Where(entry => entry.Level == LogLevel.Error).ToList();

        Assert.HasCount(1, errors);
        Assert.Contains("Unhandled exception during request: GET /matches", errors[0].Message);
        Assert.AreEqual(ThrowingProxy.Message, errors[0].Exception!.Message);
    }

    [TestMethod]
    public async Task TheErrorPageIsTranslated()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var broken = Broken(factory, new LogCollector());
        using var client = Client(broken);

        await client.GetAsync("/language?culture=pt-BR&returnUrl=/");

        var error = await client.GetAsync("/no-such-page");
        var html = WebUtility.HtmlDecode(await error.Content.ReadAsStringAsync());
        var thrown = WebUtility.HtmlDecode(await (await client.GetAsync("/matches")).Content.ReadAsStringAsync());

        Assert.Contains("<h2>Página não encontrada</h2>", html);
        Assert.Contains("Voltar para a página inicial", html);
        Assert.Contains("<h2>Algo deu errado</h2>", thrown);
    }

    [TestMethod]
    public async Task AnUnknownAddressShowsTheNotFoundPage()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = Client(factory);

        var response = await client.GetAsync("/no-such-page?x=1");
        var html = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("<h2>Page not found</h2>", html);
        Assert.Contains("Error 404", html);
        Assert.Contains("href=\"/\" class=\"btn btn-primary\"", html);
        Assert.Contains("returnUrl=%2Fno-such-page%3Fx%3D1", html);
        Assert.Contains("<link rel=\"canonical\" href=\"http://localhost/no-such-page\">", html);
    }

    [TestMethod]
    public async Task APageThatAnswersNotFoundGetsTheNotFoundPage()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = Client(factory);

        var response = await client.GetAsync("/?year=1900");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("<h2>Page not found</h2>", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task TheApiTheInGamePagesAndStaticFilesKeepABareStatus()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = Client(factory);

        foreach (var address in new[] { "/api/no-such-endpoint", "/api/matches/999999", "/ingame/no-such-page", "/css/no-such-file.css", "/imgs/no-such-image.png" })
        {
            var response = await client.GetAsync(address);
            var body = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, address);
            Assert.DoesNotContain("Page not found", body, address);
            Assert.DoesNotContain("<html", body, address);
        }
    }

    [TestMethod]
    public async Task RequestsFromScriptsKeepABareStatus()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client();

        using var json = new HttpRequestMessage(HttpMethod.Get, "/no-such-page");
        json.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var htmx = new HttpRequestMessage(HttpMethod.Get, "/no-such-page");
        htmx.Headers.Add("HX-Request", "true");

        var post = await client.PostAsync("/no-such-page", new FormUrlEncodedContent([]));

        foreach (var response in new[] { await client.SendAsync(json), await client.SendAsync(htmx), post })
        {
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual(string.Empty, await response.Content.ReadAsStringAsync());
        }
    }

    [TestMethod]
    public async Task AnApiOrInGameExceptionDoesNotGetTheSitePage()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var broken = Broken(factory, new LogCollector());
        using var client = Client(broken);

        var api = await client.GetAsync("/api/ranking");
        var inGame = await client.GetAsync("/ingame");

        Assert.AreEqual(HttpStatusCode.InternalServerError, api.StatusCode);
        Assert.AreEqual(string.Empty, await api.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.InternalServerError, inGame.StatusCode);
        Assert.AreEqual("500 Internal Server Error", await inGame.Content.ReadAsStringAsync());
        Assert.AreEqual("text/plain", inGame.Content.Headers.ContentType!.MediaType);
    }

    private static WebApplicationFactory<Program> Broken(HubWebFactory factory, LogCollector logs)
    {
        return factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddScoped(_ => ThrowingProxy.Create<IMatchViewService>());
            services.AddScoped(_ => ThrowingProxy.Create<IRankingService>());
            services.AddSingleton<ILoggerProvider>(logs);
        }));
    }

    private static HttpClient Client(WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

        return client;
    }
}
