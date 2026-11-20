using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using L4D2PlayStats.Hub.Web;
using L4D2PlayStats.Hub.Web.Infrastructure.Skins;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web.Skins;

[TestClass]
public class SkinBundledAssetsTests
{
    private const int BundledSkins = 196;
    private const string AssetsUrl = "https://assets.example.test";
    private const string Bundled = "denia-magnum";
    private const string BundledThumbnail = "/imgs/skins/denia-magnum.jpg";

    [TestMethod]
    public async Task EverySkinOfTheBundledCatalogHasAThumbnailInTheSite()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var store = factory.Services.GetRequiredService<SkinThumbnailStore>();
        var ids = SkinCatalogReader.Read(SkinBundledCatalog.Json)!.Select(skin => skin.Id!).ToList();
        var missing = ids.Where(id => !store.IsBundled(id)).ToList();
        var response = await client.GetAsync(BundledThumbnail);

        Assert.IsEmpty(missing, $"Skins without a thumbnail: {string.Join(", ", missing)}");
        Assert.HasCount(BundledSkins, store.Bundled);
        Assert.IsTrue(store.Bundled.All(ids.Contains));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("image/jpeg", response.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    public void AThumbnailComesFromTheSiteThenFromTheAssetsAddress()
    {
        using var factory = new HubWebFactory();

        var store = factory.Services.GetRequiredService<SkinThumbnailStore>();

        Assert.AreEqual(BundledThumbnail, store.Url(null, Bundled));
        Assert.AreEqual(BundledThumbnail, store.Url(AssetsUrl, Bundled));
        Assert.AreEqual($"{AssetsUrl}/skins/gold-magnum.jpg", store.Url($"{AssetsUrl}/", SkinSamples.GoldMagnum));
        Assert.AreEqual($"{CoreConsts.SkinDefaultAssetsUrl}/skins/gold-magnum.jpg", store.Url(null, SkinSamples.GoldMagnum));
        Assert.AreEqual($"{CoreConsts.SkinDefaultAssetsUrl}/skins/gold-magnum.jpg", store.Url(" ", SkinSamples.GoldMagnum));
        Assert.IsNull(store.Url("ftp://assets.example.test", SkinSamples.GoldMagnum));
        Assert.AreEqual(AppConsts.SkinEmptyImage, store.Image("ftp://assets.example.test", new Skin { Id = SkinSamples.GoldMagnum }));
        Assert.AreEqual(BundledThumbnail, store.Image("ftp://assets.example.test", new Skin { Id = Bundled }));
    }

    [TestMethod]
    public void ThePhotosUseTheAssetsAddressOfTheCurrentSiteByDefault()
    {
        Assert.AreEqual("https://l4d2playstats.blob.core.windows.net/assets", CoreConsts.SkinDefaultAssetsUrl);
        Assert.AreEqual($"{CoreConsts.SkinDefaultAssetsUrl}/skin-shots/{Bundled}/", SkinAssets.ShotsFolder(null, Bundled));
        Assert.AreEqual($"{CoreConsts.SkinDefaultAssetsUrl}/skin-shots/{Bundled}/", SkinAssets.ShotsFolder(string.Empty, Bundled));
        Assert.AreEqual($"{AssetsUrl}/skin-shots/{Bundled}/", SkinAssets.ShotsFolder(AssetsUrl, Bundled));
    }

    [TestMethod]
    public async Task TurningTheShopOnImportsTheBundledCatalogAndTheShopShowsItsPictures()
    {
        using var factory = new HubWebFactory();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        await factory.CompleteSetupAsync();
        await HubWebFactory.PostFormAsync(admin, "/admin/modules", "/admin/modules", new Dictionary<string, string> { ["Ranking"] = "true", ["SkinShop"] = "true" });

        var skins = await factory.SkinsAsync();
        var shop = await visitor.GetStringAsync("/skins");

        await HubWebFactory.PostFormAsync(admin, "/admin/modules", "/admin/modules", new Dictionary<string, string> { ["Ranking"] = "true", ["SkinShop"] = "true" });

        Assert.HasCount(BundledSkins, skins);
        Assert.HasCount(BundledSkins, await factory.SkinsAsync());
        Assert.Contains($"src=\"{BundledThumbnail}\"", shop);
        Assert.MatchesRegex(new Regex("<meta property=\"og:image\" content=\"http://localhost/imgs/skins/[a-z0-9-]+\\.jpg\">"), shop);
        Assert.Contains($"class=\"sk-gallery\" data-shots=\"{CoreConsts.SkinDefaultAssetsUrl}/skin-shots/{Bundled}/\" data-version=\"3\"", shop);
        Assert.DoesNotContain($"{CoreConsts.SkinDefaultAssetsUrl}/skins/", shop);
    }

    [TestMethod]
    public async Task TurningTheShopOffDoesNotImportTheBundledCatalog()
    {
        using var factory = new HubWebFactory();
        using var admin = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await HubWebFactory.PostFormAsync(admin, "/admin/modules", "/admin/modules", new Dictionary<string, string> { ["Ranking"] = "true" });

        Assert.IsEmpty(await factory.SkinsAsync());
    }

    [TestMethod]
    public async Task StartingTheHubWithTheShopOnAndNoCatalogImportsTheBundledCatalog()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { SkinShop = true });

        var before = await factory.SkinsAsync();

        using var restarted = factory.WithWebHostBuilder(_ => { });
        using var scope = restarted.Services.CreateScope();

        var after = await scope.ServiceProvider.GetRequiredService<ISkinService>().ListAsync();

        Assert.IsEmpty(before);
        Assert.HasCount(BundledSkins, after);
    }

    [TestMethod]
    public async Task TheCatalogPageImportsTheBundledCatalogOverAnotherOne()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.StartSkinShopAsync();

        var page = await client.GetStringAsync(AppConsts.SkinCatalogUrl);
        var response = await HubWebFactory.PostFormAsync(client, AppConsts.SkinCatalogUrl, $"{AppConsts.SkinCatalogUrl}/bundled", []);
        var after = await client.GetStringAsync(AppConsts.SkinCatalogUrl);
        var skins = await factory.SkinsAsync();

        Assert.Contains($"action=\"{AppConsts.SkinCatalogUrl}/bundled\"", page);
        Assert.Contains("with 196 skins and their thumbnails", page);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Catalog imported: 196 skins in the file, 196 added, 0 updated and 5 no longer for sale.", after);
        Assert.HasCount(BundledSkins + SkinSamples.Count, skins);
        Assert.AreEqual(BundledSkins, skins.Count(skin => skin.Available));
    }

    [TestMethod]
    public async Task TheShopSettingsShowTheDefaultAssetsAddress()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.StartSkinShopAsync(false);

        var page = await client.GetStringAsync(AppConsts.SkinSettingsUrl);

        Assert.Contains($"id=\"AssetsUrl\" name=\"AssetsUrl\" placeholder=\"{CoreConsts.SkinDefaultAssetsUrl}\"", page);
        Assert.Contains($"Leave it empty to use {CoreConsts.SkinDefaultAssetsUrl}, where the photos of the skins are published.", page);
    }
}
