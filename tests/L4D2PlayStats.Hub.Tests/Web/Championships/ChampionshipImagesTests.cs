using System.Net;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Championships;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web.Championships;

[TestClass]
public class ChampionshipImagesTests
{
    private const long FirstPlayer = 76561198100000000;

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];

    [TestMethod]
    public async Task TheAdministratorUploadsTheBannerTheLogoAndTheGalleryAndThePublicPagesShowThem()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        var (id, url) = await CreateAsync(factory, admin, "Copa Imagens");
        var response = await SaveAsync(admin, url, Fields("Copa Imagens"), ("Banner", "banner.png", Png), ("Logo", "logo.JPG", Jpeg), ("Gallery", "one.png", Png), ("Gallery", "two.webp", Webp()));
        var championship = await ChampionshipWeb.LoadAsync(factory, id);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.MatchesRegex(@"^banner-[0-9a-f]{32}\.png$", championship.BannerImage);
        Assert.MatchesRegex(@"^logo-[0-9a-f]{32}\.jpg$", championship.LogoImage);
        Assert.HasCount(2, championship.Gallery);

        var banner = await visitor.GetAsync(ImageUrl(id, championship.BannerImage!));
        var logo = await visitor.GetAsync(ImageUrl(id, championship.LogoImage!));
        var gallery = await visitor.GetAsync(ImageUrl(id, championship.Gallery[1]));

        Assert.AreEqual(HttpStatusCode.OK, banner.StatusCode);
        Assert.AreEqual("image/png", banner.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual("nosniff", banner.Headers.GetValues("X-Content-Type-Options").Single());
        CollectionAssert.AreEqual(Png, await banner.Content.ReadAsByteArrayAsync());
        Assert.AreEqual("image/jpeg", logo.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual("image/webp", gallery.Content.Headers.ContentType!.MediaType);

        var landing = await visitor.GetStringAsync("/championships/copa-imagens");
        var list = await visitor.GetStringAsync("/championships");
        var settings = await admin.GetStringAsync(url);
        var history = await admin.GetStringAsync($"{url}?tab=history");

        Assert.Contains($"class=\"championship-hero-banner\" src=\"{ImageUrl(id, championship.BannerImage!)}\"", landing);
        Assert.Contains($"class=\"championship-hero-logo\" src=\"{ImageUrl(id, championship.LogoImage!)}\"", landing);
        Assert.Contains($"<a href=\"{ImageUrl(id, championship.Gallery[0])}\" data-lity", landing);
        Assert.Contains($"<meta property=\"og:image\" content=\"http://localhost{ImageUrl(id, championship.BannerImage!)}\"", landing);
        Assert.Contains(ImageUrl(id, championship.BannerImage!), list);
        Assert.Contains("name=\"RemoveBanner\"", settings);
        Assert.Contains("name=\"RemoveLogo\"", settings);
        Assert.Contains($"name=\"RemoveGallery\" value=\"{championship.Gallery[0]}\"", settings);
        Assert.Contains("Added the banner", history);
        Assert.Contains("Added the logo", history);
        Assert.Contains("Added images to the gallery (2)", history);
        Assert.HasCount(4, Directory.GetFiles(Folder(factory, id)));
    }

    [TestMethod]
    public async Task SavingTheSettingsWithoutFilesKeepsTheImagesAndANewBannerReplacesTheOldFile()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);

        var (id, url) = await CreateAsync(factory, admin, "Copa Troca");

        await SaveAsync(admin, url, Fields("Copa Troca"), ("Banner", "banner.png", Png), ("Gallery", "one.png", Png));

        var before = await ChampionshipWeb.LoadAsync(factory, id);

        await SaveAsync(admin, url, Fields("Copa Troca Renomeada"));

        var kept = await ChampionshipWeb.LoadAsync(factory, id);

        Assert.AreEqual("Copa Troca Renomeada", kept.Name);
        Assert.AreEqual(before.BannerImage, kept.BannerImage);
        CollectionAssert.AreEqual(before.Gallery, kept.Gallery);

        await SaveAsync(admin, url, Fields("Copa Troca Renomeada"), ("Banner", "new.jpg", Jpeg));

        var changed = await ChampionshipWeb.LoadAsync(factory, id);
        var history = await admin.GetStringAsync($"{url}?tab=history");

        Assert.AreNotEqual(before.BannerImage, changed.BannerImage);
        Assert.IsTrue(changed.BannerImage!.EndsWith(".jpg"));
        Assert.IsFalse(File.Exists(Path.Combine(Folder(factory, id), before.BannerImage!)));
        Assert.IsTrue(File.Exists(Path.Combine(Folder(factory, id), changed.BannerImage!)));
        Assert.Contains("Changed the banner", history);
    }

    [TestMethod]
    public async Task TheAdministratorRemovesEachImageAndItsFileIsDeleted()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        var (id, url) = await CreateAsync(factory, admin, "Copa Remover");

        await SaveAsync(admin, url, Fields("Copa Remover"), ("Banner", "banner.png", Png), ("Logo", "logo.png", Png), ("Gallery", "one.png", Png), ("Gallery", "two.png", Png));

        var before = await ChampionshipWeb.LoadAsync(factory, id);
        var removed = before.Gallery[0];
        var fields = Fields("Copa Remover");

        fields.Add(("RemoveBanner", "true"));
        fields.Add(("RemoveLogo", "true"));
        fields.Add(("RemoveGallery", removed));

        var response = await SaveAsync(admin, url, fields);
        var after = await ChampionshipWeb.LoadAsync(factory, id);
        var history = await admin.GetStringAsync($"{url}?tab=history");
        var landing = await visitor.GetStringAsync("/championships/copa-remover");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsNull(after.BannerImage);
        Assert.IsNull(after.LogoImage);
        CollectionAssert.AreEqual(new[] { before.Gallery[1] }, after.Gallery);
        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync(ImageUrl(id, before.BannerImage!))).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync(ImageUrl(id, removed))).StatusCode);
        CollectionAssert.AreEquivalent(new[] { before.Gallery[1] }, Directory.GetFiles(Folder(factory, id)).Select(Path.GetFileName).ToArray());
        Assert.Contains("Removed the banner", history);
        Assert.Contains("Removed the logo", history);
        Assert.Contains("Removed images from the gallery (1)", history);
        Assert.DoesNotContain("championship-hero-banner", landing);
        Assert.Contains("championship-hero-logo championship-monogram", landing);
    }

    [TestMethod]
    [DataRow("banner.svg", "png")]
    [DataRow("banner.png.exe", "png")]
    [DataRow("banner", "png")]
    [DataRow("banner.png", "html")]
    public async Task AFileThatIsNotAnImageIsRefusedAndNothingIsSaved(string fileName, string kind)
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);

        var (id, url) = await CreateAsync(factory, admin, "Copa Recusa");
        var content = kind == "png" ? Png : "<html><script>alert(1)</script></html>"u8.ToArray();

        await SaveAsync(admin, url, Fields("Copa Recusa Mudou"), ("Banner", fileName, content));

        var championship = await ChampionshipWeb.LoadAsync(factory, id);
        var page = await admin.GetStringAsync(url);

        Assert.AreEqual("Copa Recusa", championship.Name);
        Assert.IsNull(championship.BannerImage);
        Assert.IsFalse(Directory.Exists(Folder(factory, id)));
        Assert.Contains(HtmlEncoder.Default.Encode("Nothing was saved: the images must be PNG, JPG, WEBP or GIF files."), page);
    }

    [TestMethod]
    public async Task AnImageLargerThanTheLimitIsRefusedAndTheCurrentOneStays()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);

        var (id, url) = await CreateAsync(factory, admin, "Copa Grande");

        await SaveAsync(admin, url, Fields("Copa Grande"), ("Logo", "logo.png", Png));

        var before = await ChampionshipWeb.LoadAsync(factory, id);
        var large = new byte[WebConsts.ChampionshipImageMaxBytes + 1];

        Png.CopyTo(large, 0);

        await SaveAsync(admin, url, Fields("Copa Grande"), ("Logo", "large.png", large));

        var after = await ChampionshipWeb.LoadAsync(factory, id);
        var page = await admin.GetStringAsync(url);

        Assert.AreEqual(before.LogoImage, after.LogoImage);
        Assert.HasCount(1, Directory.GetFiles(Folder(factory, id)));
        Assert.Contains(HtmlEncoder.Default.Encode("Nothing was saved: an image is larger than the limit."), page);

        var exact = new byte[WebConsts.ChampionshipImageMaxBytes];

        Png.CopyTo(exact, 0);

        await SaveAsync(admin, url, Fields("Copa Grande"), ("Logo", "exact.png", exact));

        Assert.AreNotEqual(before.LogoImage, (await ChampionshipWeb.LoadAsync(factory, id)).LogoImage);
    }

    [TestMethod]
    public async Task TheGalleryTakesUpToTwelveImages()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);

        var (id, url) = await CreateAsync(factory, admin, "Copa Galeria");
        var full = Enumerable.Range(0, CoreConsts.ChampionshipGalleryMaxImages).Select(index => ("Gallery", $"image-{index}.png", Png)).ToArray();

        await SaveAsync(admin, url, Fields("Copa Galeria"), full);

        Assert.HasCount(CoreConsts.ChampionshipGalleryMaxImages, (await ChampionshipWeb.LoadAsync(factory, id)).Gallery);

        await SaveAsync(admin, url, Fields("Copa Galeria"), ("Gallery", "extra.png", Png));

        var championship = await ChampionshipWeb.LoadAsync(factory, id);
        var page = await admin.GetStringAsync(url);

        Assert.HasCount(CoreConsts.ChampionshipGalleryMaxImages, championship.Gallery);
        Assert.HasCount(CoreConsts.ChampionshipGalleryMaxImages, Directory.GetFiles(Folder(factory, id)));
        Assert.Contains(HtmlEncoder.Default.Encode("Nothing was saved: the gallery is full. Remove an image before adding another."), page);

        var fields = Fields("Copa Galeria");

        fields.Add(("RemoveGallery", championship.Gallery[0]));

        await SaveAsync(admin, url, fields, ("Gallery", "extra.png", Png));

        var swapped = await ChampionshipWeb.LoadAsync(factory, id);

        Assert.HasCount(CoreConsts.ChampionshipGalleryMaxImages, swapped.Gallery);
        Assert.DoesNotContain(championship.Gallery[0], swapped.Gallery);
    }

    [TestMethod]
    public async Task TheImagesOfADraftAreSeenOnlyByAdministratorsAndOddNamesAreNotServed()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        var (id, url) = await CreateAsync(factory, admin, "Copa Rascunho", ChampionshipStatus.Draft);

        await SaveAsync(admin, url, Fields("Copa Rascunho", ChampionshipStatus.Draft), ("Banner", "banner.png", Png));

        var banner = ImageUrl(id, (await ChampionshipWeb.LoadAsync(factory, id)).BannerImage!);

        File.WriteAllText(Path.Combine(factory.DataDirectory, "secret.png"), "secret");

        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync(banner)).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (await admin.GetAsync(banner)).StatusCode);

        foreach (var address in new[] { $"{WebConsts.ChampionshipImagesUrl}/{id}/..%2F..%2F..%2Fsecret.png", $"{WebConsts.ChampionshipImagesUrl}/{id}/missing.png", $"{WebConsts.ChampionshipImagesUrl}/{id}/banner.svg", $"{WebConsts.ChampionshipImagesUrl}/{id + 1}/{banner.Split('/').Last()}" })
            Assert.AreEqual(HttpStatusCode.NotFound, (await admin.GetAsync(address)).StatusCode, address);
    }

    [TestMethod]
    public async Task DeletingTheChampionshipDeletesItsImages()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);

        var (id, url) = await CreateAsync(factory, admin, "Copa Apagada");

        await SaveAsync(admin, url, Fields("Copa Apagada"), ("Banner", "banner.png", Png), ("Gallery", "one.png", Png));

        Assert.IsTrue(Directory.Exists(Folder(factory, id)));

        var deleted = await HubWebFactory.PostFormAsync(admin, "/admin/championships", $"/admin/championships/{id}/delete", []);

        Assert.AreEqual(HttpStatusCode.Redirect, deleted.StatusCode);
        Assert.IsFalse(Directory.Exists(Folder(factory, id)));
    }

    [TestMethod]
    public async Task TheLandingShowsTheWatchLinksTheNextGameTheTeamsTheParticipantsTheFormatAndThePodium()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var visitor = factory.Client();

        var (slug, seriesId) = await factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();
            var teamService = services.GetRequiredService<IChampionshipTeamService>();
            var stageService = services.GetRequiredService<IChampionshipStageService>();
            var seriesService = services.GetRequiredService<IChampionshipSeriesService>();
            var loader = services.GetRequiredService<IChampionshipLoadService>();
            var championship = await championships.CreateAsync("Copa Vitrine", [new ChampionshipStageTemplate("Final", ChampionshipStageType.Knockout, 1, 0, 1, ChampionshipCampaignRule.Manual, ChampionshipSeeding.Seeds)], HubWebFactory.Administrator);

            await championships.UpdateAsync(championship.Id, new ChampionshipSettingsForm
            {
                Name = "Copa Vitrine", Description = "The best four players of the server.", PlayersPerGame = 2, RosterSize = 2, GameConfiguration = "ZoneMod 2.9",
                StreamUrl = "https://www.twitch.tv/example", VideosUrl = "https://www.youtube.com/@example", StartsOn = new DateTime(2026, 12, 1), EndsOn = new DateTime(2026, 12, 20)
            }, null);
            await teamService.AddParticipantsAsync(championship.Id, Enumerable.Range(0, 4).Select(index => FirstPlayer + index).ToList(), null);

            var teamA = await teamService.CreateTeamAsync(championship.Id, "Alpha Wolves", "ALP", null);
            var teamB = await teamService.CreateTeamAsync(championship.Id, "Bravo Team", null, null);

            for (var index = 0; index < 4; index++)
                await teamService.AddMemberAsync(index < 2 ? teamA.Id : teamB.Id, FirstPlayer + index, null);

            await teamService.SetLeadersAsync(teamA.Id, FirstPlayer, FirstPlayer + 1, null);
            await teamService.SetLeadersAsync(teamB.Id, FirstPlayer + 2, null, null);

            var stage = (await loader.LoadAsync(championship.Id, false))!.Stages.Single();

            await stageService.GenerateAsync(stage.Id, null);

            var series = (await loader.LoadAsync(championship.Id, false))!.Stages.Single().Series.Single();

            await seriesService.SetScheduleAsync(series.Id, DateTime.UtcNow.AddDays(3), null, null, null);

            return (championship.Slug, series.Id);
        });

        var landing = await visitor.GetStringAsync($"/championships/{slug}");

        Assert.Contains("class=\"championship-hero championship-hero-full", landing);
        Assert.Contains("<h1 class=\"championship-hero-title\">Copa Vitrine</h1>", landing);
        Assert.Contains("The best four players of the server.", landing);
        Assert.Contains("ZoneMod 2.9", landing);
        Assert.Contains("href=\"https://www.twitch.tv/example\"", landing);
        Assert.Contains("<small>twitch.tv</small>", landing);
        Assert.Contains("<small>youtube.com</small>", landing);
        Assert.MatchesRegex("data-countdown=\"\\d{4}-\\d{2}-\\d{2}T[^\"]+Z\"", landing);
        Assert.Contains($"href=\"/championships/{slug}/series/{seriesId}/calendar.ics\"", landing);
        Assert.Contains(">ALP</span>", landing);
        Assert.Contains(">BT</span>", landing);
        Assert.Contains("Alpha Wolves", landing);
        Assert.Contains($"href=\"/player/{FirstPlayer}\"", landing);
        Assert.AreEqual(4, Regex.Matches(landing, "class=\"championship-person\"").Count);
        Assert.Contains("championship-stage state-running", landing);
        Assert.DoesNotContain("id=\"podium\"", landing);
        Assert.DoesNotContain("id=\"gallery\"", landing);
        Assert.Contains("/js/pages/championships-overview.js", landing);

        await factory.RunAsync(async services =>
        {
            var loader = services.GetRequiredService<IChampionshipLoadService>();
            var seriesService = services.GetRequiredService<IChampionshipSeriesService>();
            var championship = (await loader.LoadBySlugAsync(slug))!;
            var game = championship.Stages.Single().Series.Single().Games.Single();

            await seriesService.SetManualResultAsync(game.Id, 800, 300, DateTime.UtcNow, null);
            await services.GetRequiredService<IChampionshipService>().SetStatusAsync(championship.Id, ChampionshipStatus.Finished, null, null);
        });

        var finished = await visitor.GetStringAsync($"/championships/{slug}");

        Assert.Contains("id=\"podium\"", finished);
        Assert.Contains("class=\"championship-hero-champion\"", finished);
        Assert.Contains("championship-stage state-finished", finished);
        Assert.DoesNotContain("data-countdown=", finished);
    }

    private static async Task<(int Id, string Url)> CreateAsync(HubWebFactory factory, HttpClient admin, string name, ChampionshipStatus status = ChampionshipStatus.Participants)
    {
        var created = await HubWebFactory.PostFormAsync(admin, "/admin/championships", "/admin/championships", new Dictionary<string, string> { ["name"] = name, ["template"] = "Knockout" });
        var url = created.Headers.Location!.OriginalString;
        var id = int.Parse(url.Split('/').Last());

        if (status != ChampionshipStatus.Draft)
            await factory.RunAsync(services => services.GetRequiredService<IChampionshipService>().SetStatusAsync(id, status, null, null));

        return (id, url);
    }

    private static List<(string Name, string Value)> Fields(string name, ChampionshipStatus status = ChampionshipStatus.Participants)
    {
        return
        [
            ("Name", name), ("Slug", string.Empty), ("PlayersPerGame", "4"), ("RosterSize", "4"), ("DraftMethod", "Manual"), ("Substitutes", "false"), ("status", status.ToString())
        ];
    }

    private static Task<HttpResponseMessage> SaveAsync(HttpClient client, string url, List<(string Name, string Value)> fields, params (string Field, string FileName, byte[] Content)[] files)
    {
        return HubWebFactory.PostMultipartAsync(client, url, url, fields, files);
    }

    private static byte[] Webp()
    {
        var webp = new byte[16];

        "RIFF\0\0\0\0WEBP"u8.CopyTo(webp);

        return webp;
    }

    private static string ImageUrl(int id, string file)
    {
        return $"{WebConsts.ChampionshipImagesUrl}/{id}/{file}";
    }

    private static string Folder(HubWebFactory factory, int id)
    {
        return Path.Combine(factory.DataDirectory, WebConsts.BrandingFolder, WebConsts.ChampionshipImagesFolder, id.ToString());
    }

    private static Task<Championship> LoadAsync(HubWebFactory factory, int id)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Championships.AsNoTracking().SingleAsync(item => item.Id == id));
    }
}
