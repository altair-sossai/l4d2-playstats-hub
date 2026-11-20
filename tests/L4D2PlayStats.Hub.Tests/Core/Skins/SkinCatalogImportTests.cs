using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core.Skins;

[TestClass]
public class SkinCatalogImportTests
{
    private const string NewSkin = "toxic-axe";
    private const string Axe = "weapon_melee_fireaxe";

    [TestMethod]
    public async Task TheCatalogFileOfTheAssetsRepositoryIsImported()
    {
        using var hub = new TestHub();

        var result = await hub.ImportSkinsAsync();
        var skins = await hub.SkinsAsync();
        var magnum = skins.Single(skin => skin.Id == SkinSamples.GoldMagnum);

        Assert.AreEqual(SkinCatalogImportStatus.Success, result.Status);
        Assert.AreEqual(SkinSamples.Count, result.Total);
        Assert.AreEqual(SkinSamples.Count, result.Added);
        Assert.AreEqual(0, result.Updated);
        Assert.AreEqual(0, result.Removed);
        Assert.HasCount(SkinSamples.Count, skins);
        Assert.AreEqual(SkinSamples.Magnum, magnum.Weapon);
        Assert.AreEqual(SkinRarity.Legendary, magnum.Rarity);
        Assert.AreEqual(3, magnum.DefaultPrice);
        Assert.AreEqual(3, magnum.Price);
        Assert.AreEqual("1000000003", magnum.WorkshopId);
        Assert.AreEqual("Another Author", magnum.AuthorName);
        Assert.AreEqual(76561198000000002, magnum.AuthorCommunityId);
        Assert.AreEqual("Gold Desert Eagle", magnum.OriginalTitle);
        Assert.AreEqual(2, magnum.Shots);
        Assert.IsTrue(magnum.Available);
        Assert.AreEqual("!skin gold-magnum", magnum.Command);
    }

    [TestMethod]
    [DataRow("en-US", "Golden Magnum", "A Magnum plated in gold.")]
    [DataRow("pt-BR", "Magnum Dourada", "Uma Magnum banhada a ouro.")]
    [DataRow("es-ES", "Magnum Dorada", "Una Magnum bañada en oro.")]
    [DataRow("fr-FR", "Golden Magnum", "A Magnum plated in gold.")]
    public async Task TheTextsComeInTheLanguageOfTheVisitor(string culture, string title, string description)
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var skins = await hub.SkinsAsync();
        var magnum = skins.Single(skin => skin.Id == SkinSamples.GoldMagnum);

        Assert.AreEqual(title, magnum.Title(new CultureInfo(culture)));
        Assert.AreEqual(description, magnum.Description(new CultureInfo(culture)));
    }

    [TestMethod]
    public async Task ImportingTheSameCatalogAgainChangesNothing()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var before = await hub.SkinsAsync();

        hub.Clock.Advance(TimeSpan.FromDays(1));

        var result = await hub.ImportSkinsAsync();
        var after = await hub.SkinsAsync();
        var texts = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<SkinText>().CountAsync());

        Assert.AreEqual(SkinCatalogImportStatus.Success, result.Status);
        Assert.AreEqual(SkinSamples.Count, result.Total);
        Assert.AreEqual(0, result.Added);
        Assert.AreEqual(0, result.Updated);
        Assert.AreEqual(0, result.Removed);
        Assert.HasCount(SkinSamples.Count, after);
        Assert.AreEqual(SkinSamples.Count * 3, texts);
        CollectionAssert.AreEqual(before.Select(skin => skin.UpdatedAtUtc).ToList(), after.Select(skin => skin.UpdatedAtUtc).ToList());
    }

    [TestMethod]
    public async Task ASkinThatChangedInTheFileIsUpdated()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var changed = SkinSamples.Catalog(skins =>
        {
            var magnum = SkinSamples.Find(skins, SkinSamples.GoldMagnum);
            var title = magnum["title"]!.AsObject();
            var description = magnum["description"]!.AsObject();

            magnum["price"] = 9;
            magnum["rarity"] = "epic";
            magnum["shots"] = 4;
            title["pt"] = "Magnum de Ouro";
            title.Remove("es");
            description.Remove("es");
        });

        var result = await hub.ImportSkinsAsync(changed);
        var skins = await hub.SkinsAsync();
        var magnum = skins.Single(skin => skin.Id == SkinSamples.GoldMagnum);

        Assert.AreEqual(0, result.Added);
        Assert.AreEqual(1, result.Updated);
        Assert.AreEqual(0, result.Removed);
        Assert.HasCount(SkinSamples.Count, skins);
        Assert.AreEqual(9, magnum.Price);
        Assert.AreEqual(SkinRarity.Epic, magnum.Rarity);
        Assert.AreEqual(4, magnum.Shots);
        Assert.AreEqual("Magnum de Ouro", magnum.Title(new CultureInfo("pt-BR")));
        Assert.AreEqual("Golden Magnum", magnum.Title(new CultureInfo("es-ES")));
        CollectionAssert.AreEquivalent(new[] { "en", "pt" }, magnum.Texts.Select(text => text.Language).ToList());
    }

    [TestMethod]
    public async Task ASkinThatIsNewInTheFileIsAdded()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var result = await hub.ImportSkinsAsync(SkinSamples.Catalog(skins => skins.Add(SkinSamples.NewSkin(NewSkin, Axe, 7))));
        var skins = await hub.SkinsAsync();
        var axe = skins.Single(skin => skin.Id == NewSkin);

        Assert.AreEqual(1, result.Added);
        Assert.AreEqual(0, result.Updated);
        Assert.AreEqual(0, result.Removed);
        Assert.HasCount(SkinSamples.Count + 1, skins);
        Assert.AreEqual(Axe, axe.Weapon);
        Assert.AreEqual(7, axe.Price);
        Assert.AreEqual(SkinRarity.Uncommon, axe.Rarity);
        Assert.IsTrue(axe.Available);
    }

    [TestMethod]
    public async Task ASkinRemovedFromTheFileStopsBeingSoldButStaysWithItsOwners()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var owner = await hub.PlayerWithStarsAsync();

        await hub.ShopAsync(shop => shop.BuyAsync(owner, "Owner", SkinSamples.DesertSmg));

        var result = await hub.ImportSkinsAsync(SkinSamples.Catalog(skins => skins.Remove(SkinSamples.Find(skins, SkinSamples.DesertSmg))));
        var skins = await hub.SkinsAsync();
        var owned = await hub.ShopAsync(shop => shop.GetPlayerSkinsAsync(owner));
        var buy = await hub.ShopAsync(shop => shop.BuyAsync(StatisticsBuilder.PlayerA1, null, SkinSamples.DesertSmg));
        var gift = await hub.ShopAsync(shop => shop.GiftAsync(StatisticsBuilder.PlayerA1, SkinSamples.DesertSmg, null, null));
        var unequip = await hub.ShopAsync(shop => shop.UnequipAsync(owner, SkinSamples.Smg));
        var equip = await hub.ShopAsync(shop => shop.EquipAsync(owner, SkinSamples.DesertSmg, "site"));
        var purchased = await hub.ShopAsync(shop => shop.GetPurchasedSkinsAsync());

        Assert.AreEqual(0, result.Added);
        Assert.AreEqual(0, result.Updated);
        Assert.AreEqual(1, result.Removed);
        Assert.HasCount(SkinSamples.Count, skins);
        Assert.IsFalse(skins.Single(skin => skin.Id == SkinSamples.DesertSmg).Available);
        Assert.AreEqual(SkinSamples.Count - 1, skins.Count(skin => skin.Available));
        Assert.IsTrue(owned.Owns(SkinSamples.DesertSmg));
        Assert.AreEqual(SkinSamples.DesertSmg, owned.Equipped[SkinSamples.Smg]);
        Assert.AreEqual(SkinShopStatus.SkinNotFound, buy.Status);
        Assert.AreEqual(SkinShopStatus.SkinNotFound, gift.Status);
        Assert.AreEqual(SkinShopStatus.Success, unequip.Status);
        Assert.AreEqual(SkinShopStatus.Success, equip.Status);
        Assert.AreEqual(SkinSamples.DesertSmg, purchased.Single().Id);
    }

    [TestMethod]
    public async Task ARemovedSkinThatComesBackIsSoldAgain()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();
        await hub.ImportSkinsAsync(SkinSamples.Catalog(skins => skins.Remove(SkinSamples.Find(skins, SkinSamples.DesertSmg))));

        var removedAgain = await hub.ImportSkinsAsync(SkinSamples.Catalog(skins => skins.Remove(SkinSamples.Find(skins, SkinSamples.DesertSmg))));
        var result = await hub.ImportSkinsAsync();
        var skins = await hub.SkinsAsync();

        Assert.AreEqual(0, removedAgain.Removed);
        Assert.AreEqual(0, result.Added);
        Assert.AreEqual(1, result.Updated);
        Assert.IsTrue(skins.All(skin => skin.Available));
    }

    [TestMethod]
    public async Task APriceSetByAnAdministratorSurvivesAnImport()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();
        await hub.RunAsync(services => services.GetRequiredService<ISkinService>().SetPriceAsync(SkinSamples.GoldMagnum, 50, null));

        var cheaper = SkinSamples.Catalog(skins =>
        {
            var skin = SkinSamples.Find(skins, SkinSamples.GoldMagnum);

            skin["price"] = 4;
        });

        var result = await hub.ImportSkinsAsync(cheaper);
        var skins = await hub.SkinsAsync();
        var magnum = skins.Single(skin => skin.Id == SkinSamples.GoldMagnum);

        Assert.AreEqual(1, result.Updated);
        Assert.AreEqual(4, magnum.DefaultPrice);
        Assert.AreEqual(50, magnum.Price);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("this is not a catalog")]
    [DataRow("{}")]
    [DataRow("[]")]
    [DataRow("{\"skins\":[null]}")]
    [DataRow("{\"skins\":[{\"weapon\":\"weapon_smg\",\"rarity\":\"common\",\"price\":1}]}")]
    [DataRow("{\"skins\":[{\"id\":\"a\",\"rarity\":\"common\",\"price\":1}]}")]
    [DataRow("{\"skins\":[{\"id\":\"a\",\"weapon\":\"weapon_smg\",\"price\":1}]}")]
    [DataRow("{\"skins\":[{\"id\":\"a\",\"weapon\":\"weapon_smg\",\"rarity\":\"mythic\",\"price\":1}]}")]
    [DataRow("{\"skins\":[{\"id\":\"a\",\"weapon\":\"weapon_smg\",\"rarity\":\"common\"}]}")]
    [DataRow("{\"skins\":[{\"id\":\"a\",\"weapon\":\"weapon_smg\",\"rarity\":\"common\",\"price\":0}]}")]
    [DataRow("{\"skins\":[{\"id\":\"a\",\"weapon\":\"weapon_smg\",\"rarity\":\"common\",\"price\":1},{\"id\":\"a\",\"weapon\":\"weapon_smg\",\"rarity\":\"rare\",\"price\":2}]}")]
    public async Task AFileThatIsNotACatalogIsRefusedAndChangesNothing(string? json)
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var result = await hub.RunAsync(services => services.GetRequiredService<ISkinCatalogService>().ImportAsync(json));
        var skins = await hub.SkinsAsync();

        Assert.AreEqual(SkinCatalogImportStatus.InvalidFile, result.Status);
        Assert.IsFalse(result.Succeeded);
        Assert.HasCount(SkinSamples.Count, skins);
        Assert.IsTrue(skins.All(skin => skin.Available));
    }

    [TestMethod]
    public async Task ACatalogWithoutSkinsIsRefusedSoNothingGoesOffSaleByMistake()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var result = await hub.ImportSkinsAsync("{\"skins\":[]}");
        var skins = await hub.SkinsAsync();

        Assert.AreEqual(SkinCatalogImportStatus.EmptyCatalog, result.Status);
        Assert.IsTrue(skins.All(skin => skin.Available));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("skins.json")]
    [DataRow("ftp://assets.example.test/skins.json")]
    public async Task ImportingFromAnAddressNeedsAWebAddressInTheSettings(string? url)
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SkinShopSettings { CatalogUrl = url }));

        var result = await hub.RunAsync(services => services.GetRequiredService<ISkinCatalogService>().ImportFromUrlAsync());

        Assert.AreEqual(SkinCatalogImportStatus.UrlNotConfigured, result.Status);
    }
}