using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Tests.Support;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class StaticFileCachingTests
{
    [TestMethod]
    public async Task AVersionedFileIsCachedForAYearAndAnUnversionedScriptIsAlwaysChecked()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var page = await client.GetStringAsync("/commands");
        var versioned = Regex.Match(page, @"/js/hub\.js\?v=[^""]+").Value;

        using var withVersion = await client.GetAsync(versioned);
        using var script = await client.GetAsync("/js/hub.js");
        using var style = await client.GetAsync("/css/admin.css");
        using var image = await client.GetAsync("/favicon.ico");

        Assert.IsFalse(string.IsNullOrEmpty(versioned));
        Assert.AreEqual("public, max-age=31536000, immutable", withVersion.Headers.CacheControl?.ToString());
        Assert.AreEqual("no-cache", script.Headers.CacheControl?.ToString());
        Assert.AreEqual("no-cache", style.Headers.CacheControl?.ToString());
        Assert.AreEqual("public, max-age=3600", image.Headers.CacheControl?.ToString());
    }

    [TestMethod]
    public async Task EveryLocalStyleAndScriptOfAPageCarriesAVersion()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        foreach (var path in new[] { "/", "/players", "/matches", "/commands", "/patents", "/admin", "/admin/settings", "/admin/campaigns", "/admin/forced-names" })
        {
            var page = await client.GetStringAsync(path);
            var references = Regex.Matches(page, "(?:href|src)=\"(/(?:css|js|lib)/[^\"]+)\"");

            foreach (Match reference in references)
                Assert.Contains("?v=", reference.Groups[1].Value, $"{path}: {reference.Groups[1].Value}");
        }
    }
}
