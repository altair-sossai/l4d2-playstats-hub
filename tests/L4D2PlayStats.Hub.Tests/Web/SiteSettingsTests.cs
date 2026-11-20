using System.Net;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Infrastructure.Branding;
using Microsoft.Extensions.DependencyInjection;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class SiteSettingsTests
{
    private const string SettingsPage = "/admin/settings";
    private const string HomePage = "/";
    private const string PngContentType = "image/png";

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03];

    [TestMethod]
    public async Task TheAdministratorSavesTheFooterLinksTheKeywordsAndTheAnalyticsId()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string>
        {
            ["Site.FooterLinks"] = "  Discord|https://discord.gg/example \r\n\r\nSteam group | https://steamcommunity.com/groups/example",
            ["Site.MetaKeywords"] = " left 4 dead 2 ,ranking,\n statistics ",
            ["Site.GoogleAnalyticsId"] = " G-TEST12345 "
        }));

        var site = await SiteAsync(factory);
        var home = await client.GetStringAsync(HomePage);
        var settings = await client.GetStringAsync(SettingsPage);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("Discord | https://discord.gg/example\nSteam group | https://steamcommunity.com/groups/example", site.FooterLinks);
        Assert.AreEqual("left 4 dead 2, ranking, statistics", site.MetaKeywords);
        Assert.AreEqual("G-TEST12345", site.GoogleAnalyticsId);
        Assert.Contains("href=\"https://discord.gg/example\"", home);
        Assert.Contains("<small>Steam group</small>", home);
        Assert.DoesNotContain(WebConsts.ProjectUrl, home);
        Assert.Contains("googletagmanager.com/gtag/js?id=G-TEST12345", home);
        Assert.Contains("value=\"G-TEST12345\"", settings);
        Assert.Contains("value=\"left 4 dead 2, ranking, statistics\"", settings);
        Assert.Contains("Steam group | https://steamcommunity.com/groups/example</textarea>", settings);
    }

    [TestMethod]
    public async Task WithoutTheNewSettingsThePageLinksToTheProjectAndLoadsNoAnalytics()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string>
        {
            ["Site.FooterLinks"] = "Discord | https://discord.gg/example",
            ["Site.MetaKeywords"] = "ranking",
            ["Site.GoogleAnalyticsId"] = "G-TEST12345"
        }));

        await PostAsync(client, SettingsPage, SettingsPage, Fields([]));

        var site = await SiteAsync(factory);
        var home = await client.GetStringAsync(HomePage);

        Assert.IsNull(site.FooterLinks);
        Assert.IsNull(site.MetaKeywords);
        Assert.IsNull(site.GoogleAnalyticsId);
        Assert.Contains($"href=\"{WebConsts.ProjectUrl}\"", home);
        Assert.DoesNotContain("googletagmanager.com", home);
    }

    [TestMethod]
    public async Task FooterLinesThatAreNotLinksAreLeftOutAndTheAdministratorIsTold()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string>
        {
            ["Site.FooterLinks"] = "Discord | https://discord.gg/example\nScript | javascript:alert(1)\nNo address"
        }));

        var site = await SiteAsync(factory);
        var settings = await client.GetStringAsync(SettingsPage);
        var again = await client.GetStringAsync(SettingsPage);

        Assert.AreEqual("Discord | https://discord.gg/example", site.FooterLinks);
        Assert.Contains(HtmlEncoder.Default.Encode(WebConsts.FooterLinksIgnoredMessage), settings);
        Assert.DoesNotContain("javascript:alert", settings);
        Assert.DoesNotContain(HtmlEncoder.Default.Encode(WebConsts.FooterLinksIgnoredMessage), again);
    }

    [TestMethod]
    [DataRow("G-TEST<script>")]
    [DataRow("G TEST")]
    [DataRow("G-TEST\"onload=\"x")]
    public async Task AnAnalyticsIdWithOtherCharactersIsRefusedAndThePreviousOneStays(string analyticsId)
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["Site.GoogleAnalyticsId"] = "G-GOOD1" }));
        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["Site.GoogleAnalyticsId"] = analyticsId }));

        var site = await SiteAsync(factory);
        var settings = await client.GetStringAsync(SettingsPage);

        Assert.AreEqual("G-GOOD1", site.GoogleAnalyticsId);
        Assert.Contains(HtmlEncoder.Default.Encode(WebConsts.GoogleAnalyticsIdInvalidMessage), settings);
    }

    [TestMethod]
    public async Task TheKeywordsAreCutAtTheLimit()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["Site.MetaKeywords"] = new('a', WebConsts.MetaKeywordsMaxLength + 50) }));

        var site = await SiteAsync(factory);

        Assert.AreEqual(WebConsts.MetaKeywordsMaxLength, site.MetaKeywords!.Length);
    }

    [TestMethod]
    public async Task WithoutALogoTheMenuShowsTheSiteName()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var home = await client.GetStringAsync(HomePage);
        var settings = await client.GetStringAsync(SettingsPage);

        Assert.AreEqual(HttpStatusCode.NotFound, logo.StatusCode);
        Assert.Contains("class=\"navbar-brand site-name\"", home);
        Assert.DoesNotContain(WebConsts.LogoUrl, home);
        Assert.Contains("name=\"Logo\"", settings);
        Assert.Contains("enctype=\"multipart/form-data\"", settings);
        Assert.DoesNotContain("name=\"RemoveLogo\"", settings);
    }

    [TestMethod]
    public async Task TheAdministratorUploadsTheLogoAndTheMenuShowsIt()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await PostAsync(client, SettingsPage, SettingsPage, Fields([]), "logo.PNG", Png);
        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var content = await logo.Content.ReadAsByteArrayAsync();
        var home = await client.GetStringAsync(HomePage);
        var settings = await client.GetStringAsync(SettingsPage);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, logo.StatusCode);
        Assert.AreEqual(PngContentType, logo.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual("nosniff", logo.Headers.GetValues("X-Content-Type-Options").Single());
        CollectionAssert.AreEqual(Png, content);
        Assert.Contains($"<img src=\"{WebConsts.LogoUrl}?v=", home);
        Assert.DoesNotContain("class=\"navbar-brand site-name\"", home);
        Assert.Contains("name=\"RemoveLogo\"", settings);
    }

    [TestMethod]
    public async Task ANewLogoReplacesTheOldOneEvenWithAnotherFileType()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        byte[] second = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61];

        await PostAsync(client, SettingsPage, SettingsPage, Fields([]), "logo.png", Png);
        await PostAsync(client, SettingsPage, SettingsPage, Fields([]), "other.gif", second);

        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var content = await logo.Content.ReadAsByteArrayAsync();

        Assert.AreEqual("image/gif", logo.Content.Headers.ContentType!.MediaType);
        CollectionAssert.AreEqual(second, content);
    }

    [TestMethod]
    public async Task TheSettingsPageAsksWhoCanSeeRelatedAccountsAndStartsWithEveryone()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var html = await client.GetStringAsync(SettingsPage);

        Assert.Contains("<h3 class=\"panel-title\">Privacy</h3>", html);
        Assert.Contains("<label for=\"Privacy_RelatedAccounts\">Who can see related accounts</label>", html);
        Assert.Contains("<option value=\"Everyone\" selected=\"selected\">Everyone</option>", html);
        Assert.Contains("<option value=\"Administrators\">Administrators only</option>", html);
        Assert.Contains("gameinfo_related_accounts_chat", html);
    }

    [TestMethod]
    public async Task TheAdministratorLimitsRelatedAccountsToAdministratorsAndBack()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["Privacy.RelatedAccounts"] = "Administrators" }));

        var limited = await PrivacyAsync(factory);
        var html = await client.GetStringAsync(SettingsPage);

        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["Privacy.RelatedAccounts"] = "Everyone" }));

        var everyone = await PrivacyAsync(factory);

        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["Privacy.RelatedAccounts"] = "7" }));

        var invalid = await PrivacyAsync(factory);

        Assert.AreEqual(RelatedAccountsVisibility.Administrators, limited.RelatedAccounts);
        Assert.Contains("<option value=\"Administrators\" selected=\"selected\">Administrators only</option>", html);
        Assert.AreEqual(RelatedAccountsVisibility.Everyone, everyone.RelatedAccounts);
        Assert.AreEqual(RelatedAccountsVisibility.Everyone, invalid.RelatedAccounts);
    }

    [TestMethod]
    public async Task SavingTheSettingsWithoutAFileKeepsTheLogo()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields([]), "logo.png", Png);
        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["Site.Name"] = "Another Name" }));

        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var site = await SiteAsync(factory);

        Assert.AreEqual(HttpStatusCode.OK, logo.StatusCode);
        Assert.AreEqual("Another Name", site.Name);
    }

    [TestMethod]
    public async Task TheAdministratorRemovesTheLogoAndTheSiteNameComesBack()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields([]), "logo.png", Png);

        var response = await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["RemoveLogo"] = "true" }));
        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var home = await client.GetStringAsync(HomePage);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, logo.StatusCode);
        Assert.Contains("class=\"navbar-brand site-name\"", home);
        Assert.IsFalse(factory.Services.GetRequiredService<BrandingStore>().HasLogo);
    }

    [TestMethod]
    [DataRow("logo.svg")]
    [DataRow("logo.png.exe")]
    [DataRow("logo")]
    public async Task ALogoThatIsNotAnImageOfAKnownTypeIsRefused(string fileName)
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields(new Dictionary<string, string> { ["Site.Name"] = "Saved Anyway" }), fileName, Png);

        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var settings = await client.GetStringAsync(SettingsPage);
        var site = await SiteAsync(factory);

        Assert.AreEqual(HttpStatusCode.NotFound, logo.StatusCode);
        Assert.Contains(HtmlEncoder.Default.Encode(WebConsts.LogoUnsupportedMessage), settings);
        Assert.AreEqual("Saved Anyway", site.Name);
    }

    [TestMethod]
    [DataRow("logo.png")]
    [DataRow("logo.jpg")]
    [DataRow("logo.gif")]
    [DataRow("logo.webp")]
    public async Task AFileThatIsNotReallyAnImageIsRefusedAsTheLogo(string fileName)
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields([]), fileName, "<html><script>alert(1)</script></html>"u8.ToArray());

        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var settings = await client.GetStringAsync(SettingsPage);

        Assert.AreEqual(HttpStatusCode.NotFound, logo.StatusCode);
        Assert.Contains(HtmlEncoder.Default.Encode(WebConsts.LogoUnsupportedMessage), settings);
    }

    [TestMethod]
    public async Task ALogoLargerThanTheLimitIsRefusedAndTheCurrentOneStays()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, SettingsPage, SettingsPage, Fields([]), "logo.png", Png);
        await PostAsync(client, SettingsPage, SettingsPage, Fields([]), "large.png", new byte[WebConsts.LogoMaxBytes + 1]);

        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var content = await logo.Content.ReadAsByteArrayAsync();
        var settings = await client.GetStringAsync(SettingsPage);

        CollectionAssert.AreEqual(Png, content);
        Assert.Contains(HtmlEncoder.Default.Encode(WebConsts.LogoTooLargeMessage), settings);
    }

    [TestMethod]
    public async Task ALogoOfExactlyTheLimitIsAccepted()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var webp = new byte[WebConsts.LogoMaxBytes];
        "RIFF\0\0\0\0WEBP"u8.CopyTo(webp);

        await PostAsync(client, SettingsPage, SettingsPage, Fields([]), "logo.webp", webp);

        var logo = await client.GetAsync(WebConsts.LogoUrl);

        Assert.AreEqual(HttpStatusCode.OK, logo.StatusCode);
        Assert.AreEqual(WebConsts.LogoMaxBytes, logo.Content.Headers.ContentLength);
    }

    [TestMethod]
    public async Task OnlyAnAdministratorChangesTheLogo()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);

        var token = await TokenAsync(administrator, SettingsPage);

        using var content = Content(Fields([]), token, "logo.png", Png);

        var response = await player.PostAsync(SettingsPage, content);
        var logo = await player.GetAsync(WebConsts.LogoUrl);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, logo.StatusCode);
    }

    [TestMethod]
    public async Task TheWizardTakesTheLogoInTheSiteStep()
    {
        using var factory = new HubWebFactory();

        await factory.RunAsync(services => services.GetRequiredService<IAdministratorService>().AddAsync(HubWebFactory.Administrator, "Admin"));

        using var client = factory.Client(HubWebFactory.Administrator);

        var page = await client.GetStringAsync(WebConsts.SetupWizardUrl);
        var response = await PostAsync(client, WebConsts.SetupWizardUrl, "/setup/site", Fields(new Dictionary<string, string> { ["Site.Name"] = "Wizard Site" }), "logo.jpg", Png);
        var logo = await client.GetAsync(WebConsts.LogoUrl);
        var site = await SiteAsync(factory);

        Assert.Contains("name=\"Logo\"", page);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/setup?step=modules", response.Headers.Location!.OriginalString);
        Assert.AreEqual("image/jpeg", logo.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual("Wizard Site", site.Name);
    }

    [TestMethod]
    public async Task TheWizardStaysInTheSiteStepWhenTheLogoIsRefused()
    {
        using var factory = new HubWebFactory();

        await factory.RunAsync(services => services.GetRequiredService<IAdministratorService>().AddAsync(HubWebFactory.Administrator, "Admin"));

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await PostAsync(client, WebConsts.SetupWizardUrl, "/setup/site", Fields([]), "logo.svg", Png);
        var page = await client.GetStringAsync(WebConsts.SetupWizardUrl);
        var logo = await client.GetAsync(WebConsts.LogoUrl);

        Assert.AreEqual(WebConsts.SetupWizardUrl, response.Headers.Location!.OriginalString);
        Assert.Contains(HtmlEncoder.Default.Encode(WebConsts.LogoUnsupportedMessage), page);
        Assert.Contains("action=\"/setup/site\"", page);
        Assert.AreEqual(HttpStatusCode.NotFound, logo.StatusCode);
    }

    [TestMethod]
    public async Task TheWizardKeepsTheSettingsItDoesNotAskFor()
    {
        using var factory = new HubWebFactory();

        await factory.RunAsync(services => services.GetRequiredService<IAdministratorService>().AddAsync(HubWebFactory.Administrator, "Admin"));
        await factory.SaveAsync(new SiteSettings { FooterLinks = "Discord | https://discord.gg/example", GoogleAnalyticsId = "G-KEEP", MetaKeywords = "ranking" });

        using var client = factory.Client(HubWebFactory.Administrator);

        await PostAsync(client, WebConsts.SetupWizardUrl, "/setup/site", Fields(new Dictionary<string, string> { ["Site.Name"] = "Wizard Site" }));

        var site = await SiteAsync(factory);

        Assert.AreEqual("Wizard Site", site.Name);
        Assert.AreEqual("Discord | https://discord.gg/example", site.FooterLinks);
        Assert.AreEqual("G-KEEP", site.GoogleAnalyticsId);
        Assert.AreEqual("ranking", site.MetaKeywords);
    }

    private static Dictionary<string, string> Fields(Dictionary<string, string> changes)
    {
        var fields = new Dictionary<string, string>
        {
            ["Site.Name"] = "Test Site",
            ["Site.DefaultCulture"] = "en-US",
            ["Season.LengthInMonths"] = "2"
        };

        foreach (var change in changes)
            fields[change.Key] = change.Value;

        return fields;
    }

    private static Task<PrivacySettings> PrivacyAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<PrivacySettings>());
    }

    private static Task<SiteSettings> SiteAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SiteSettings>());
    }

    private static async Task<string> TokenAsync(HttpClient client, string formPage)
    {
        var html = await client.GetStringAsync(formPage);

        return Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
    }

    private static MultipartFormDataContent Content(Dictionary<string, string> fields, string token, string? fileName, byte[]? file)
    {
        var content = new MultipartFormDataContent { { new StringContent(token), "__RequestVerificationToken" } };

        foreach (var field in fields)
            content.Add(new StringContent(field.Value), field.Key);

        if (fileName == null || file == null)
            return content;

        var part = new ByteArrayContent(file);

        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(part, "Logo", fileName);

        return content;
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string formPage, string action, Dictionary<string, string> fields, string? fileName = null, byte[]? file = null)
    {
        var token = await TokenAsync(client, formPage);

        using var content = Content(fields, token, fileName, file);

        return await client.PostAsync(action, content);
    }
}