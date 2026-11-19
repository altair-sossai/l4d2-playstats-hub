using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core.Skins;

[TestClass]
public class SkinBundledCatalogTests
{
    private const int BundledSkins = 196;
    private const string Magnum = "denia-magnum";
    private const long MagnumAuthor = 76561198068179093;

    [TestMethod]
    public void TheBundledCatalogIsTheCatalogOfTheCurrentSite()
    {
        var skins = SkinCatalogReader.Read(SkinBundledCatalog.Json);

        Assert.IsNotNull(skins);
        Assert.HasCount(BundledSkins, skins);
        Assert.AreEqual(BundledSkins, SkinBundledCatalog.Count);
        Assert.IsTrue(skins.All(skin => !string.IsNullOrEmpty(skin.WorkshopId) && skin.Author?.Name != null && long.TryParse(skin.Author.CommunityId, out _)));
        Assert.IsTrue(skins.All(skin => skin.Shots > 0 && skin.OriginalTitle != null));
        Assert.IsTrue(skins.All(skin => new[] { "en", "pt", "es" }.All(language => skin.Title!.ContainsKey(language) && skin.Description!.ContainsKey(language))));
    }

    [TestMethod]
    public async Task TheBundledCatalogIsImportedWithEveryField()
    {
        using var hub = new TestHub();

        var result = await hub.RunAsync(services => services.GetRequiredService<ISkinCatalogService>().ImportBundledAsync());
        var skins = await hub.SkinsAsync();
        var magnum = skins.Single(skin => skin.Id == Magnum);

        Assert.AreEqual(SkinCatalogImportStatus.Success, result.Status);
        Assert.AreEqual(BundledSkins, result.Added);
        Assert.HasCount(BundledSkins, skins);
        Assert.AreEqual("weapon_pistol_magnum", magnum.Weapon);
        Assert.AreEqual(SkinRarity.Legendary, magnum.Rarity);
        Assert.AreEqual(630, magnum.DefaultPrice);
        Assert.AreEqual("3757405260", magnum.WorkshopId);
        Assert.AreEqual(3, magnum.Shots);
        Assert.AreEqual("SSSoyorin", magnum.AuthorName);
        Assert.AreEqual(MagnumAuthor, magnum.AuthorCommunityId);
        Assert.AreEqual("Magnum Denia (Wuthering Waves)", magnum.Title(new CultureInfo("pt-BR")));
        Assert.StartsWith("Magnum animada", magnum.Description(new CultureInfo("pt-BR")));
        Assert.StartsWith("Magnum animada con", magnum.Description(new CultureInfo("es-ES")));
    }

    [TestMethod]
    public async Task TheBundledCatalogIsImportedOnlyWhenTheShopIsOnAndTheCatalogIsEmpty()
    {
        using var hub = new TestHub();

        var off = await SeedAsync(hub);

        await hub.EnableSkinShopAsync();

        var first = await SeedAsync(hub);
        var second = await SeedAsync(hub);
        var skins = await hub.SkinsAsync();

        Assert.IsNull(off);
        Assert.IsNotNull(first);
        Assert.AreEqual(BundledSkins, first.Added);
        Assert.IsNull(second);
        Assert.HasCount(BundledSkins, skins);
    }

    [TestMethod]
    public async Task TheBundledCatalogIsNotImportedOverAnotherCatalog()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();
        await hub.EnableSkinShopAsync();

        var result = await SeedAsync(hub);
        var skins = await hub.SkinsAsync();

        Assert.IsNull(result);
        Assert.HasCount(SkinSamples.Count, skins);
    }

    [TestMethod]
    public async Task TheBundledCatalogIsNotImportedWithoutTheRanking()
    {
        using var hub = new TestHub();

        await hub.EnableSkinShopAsync(false);

        var result = await SeedAsync(hub);

        Assert.IsNull(result);
        Assert.IsEmpty(await hub.SkinsAsync());
    }

    [TestMethod]
    public async Task ImportingTheBundledCatalogAgainKeepsThePricesAndTheSteamNames()
    {
        using var hub = new TestHub();

        await hub.EnableSkinShopAsync();
        await SeedAsync(hub);
        await hub.RunAsync(services => services.GetRequiredService<ISkinService>().SetPriceAsync(Magnum, 10, HubWebFactory.Administrator));
        await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.Add(new SkinAuthor { CommunityId = MagnumAuthor, Name = "Soyorin Today", SyncedAtUtc = hub.Clock.UtcNow });

            await dbContext.SaveChangesAsync();
        });

        var result = await hub.RunAsync(services => services.GetRequiredService<ISkinCatalogService>().ImportBundledAsync());
        var magnum = (await hub.SkinsAsync()).Single(skin => skin.Id == Magnum);

        Assert.AreEqual(SkinCatalogImportStatus.Success, result.Status);
        Assert.AreEqual(BundledSkins, result.Total);
        Assert.AreEqual(0, result.Added);
        Assert.AreEqual(0, result.Removed);
        Assert.AreEqual(10, magnum.Price);
        Assert.AreEqual(630, magnum.DefaultPrice);
        Assert.AreEqual("Soyorin Today", magnum.AuthorName);
    }

    private static Task<SkinCatalogImportResult?> SeedAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISkinCatalogService>().SeedAsync());
    }
}
