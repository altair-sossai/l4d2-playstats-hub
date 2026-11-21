using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Commands.Contracts;
using L4D2PlayStats.Hub.Web.Models.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Commands;

[TestClass]
public class CommandCatalogTests
{
    private const string WebProject = "src/L4D2PlayStats.Hub.Web";
    private const string TypingSection = "typing";
    private static readonly string[] Cultures = ["en", "pt-BR", "es-ES"];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [TestMethod]
    public void EveryLanguageHasTheSameSectionsAndCommands()
    {
        var english = Signature(Load("en"));

        foreach (var culture in Cultures.Skip(1))
            CollectionAssert.AreEqual(english, Signature(Load(culture)), culture);
    }

    [TestMethod]
    public void TheCommandNamesAreTheSameInEveryLanguage()
    {
        var english = Names(Load("en"));

        foreach (var culture in Cultures.Skip(1))
            CollectionAssert.AreEqual(english, Names(Load(culture)), culture);
    }

    [TestMethod]
    [DataRow("en")]
    [DataRow("pt-BR")]
    [DataRow("es-ES")]
    public void EveryCommandIsExplained(string culture)
    {
        var sections = Load(culture);
        var ids = sections.SelectMany(section => section.Commands.Select(command => command.Id)).ToList();

        Assert.IsNotEmpty(sections);
        Assert.HasCount(ids.Count, ids.Distinct());
        Assert.HasCount(sections.Count, sections.Select(section => section.Id).Distinct());

        foreach (var section in sections)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(section.Title), section.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(section.Intro), section.Id);
            Assert.IsNotEmpty(section.Commands, section.Id);

            foreach (var command in section.Commands)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(command.Description), command.Id);
                Assert.IsTrue(command.Names.Count > 0 || !string.IsNullOrWhiteSpace(command.Title), command.Id);
                Assert.IsTrue(command.Examples.All(example => !string.IsNullOrWhiteSpace(example.Code)), command.Id);
            }
        }
    }

    [TestMethod]
    public void EverySectionIconIsAGlyphicon()
    {
        var css = File.ReadAllText(Path.Combine(WebProjectDirectory(), "wwwroot", "css", "bootstrap.min.css"));

        foreach (var section in Load("en"))
            Assert.Contains($".glyphicon-{section.Icon}:before", css, section.Id);
    }

    [TestMethod]
    public void TheAdministratorCommandsComeLast()
    {
        var sections = Load("en");

        Assert.IsTrue(sections[^1].Admin);
        Assert.AreEqual(1, sections.Count(section => section.Admin));
    }

    [TestMethod]
    public void TheVisitorLanguageIsUsedAndEnglishIsTheFallback()
    {
        using var factory = new HubWebFactory();

        var catalog = factory.Services.GetRequiredService<ICommandCatalog>();

        Assert.AreEqual("How to type commands", catalog.All(new CultureInfo("en-US")).Single(section => section.Id == TypingSection).Title);
        Assert.AreEqual("Como digitar os comandos", catalog.All(new CultureInfo("pt-BR")).Single(section => section.Id == TypingSection).Title);
        Assert.AreEqual("Cómo escribir los comandos", catalog.All(new CultureInfo("es-ES")).Single(section => section.Id == TypingSection).Title);
        Assert.AreEqual("How to type commands", catalog.All(new CultureInfo("fr-FR")).Single(section => section.Id == TypingSection).Title);
        Assert.AreEqual("How to type commands", catalog.All(CultureInfo.InvariantCulture).Single(section => section.Id == TypingSection).Title);
        Assert.AreEqual("anticheat", catalog.All(new CultureInfo("pt-BR"))[0].Id);
    }

    [TestMethod]
    public void AModuleThatIsOffHidesItsCommands()
    {
        using var factory = new HubWebFactory();

        var catalog = factory.Services.GetRequiredService<ICommandCatalog>();
        var english = new CultureInfo("en-US");
        var all = catalog.All(english);
        var everything = catalog.Sections(english, new ModuleSettings { Ranking = true, SkinShop = true, CommunityRanking = true, AntiCheat = true });
        var nothing = catalog.Sections(english, new ModuleSettings { Ranking = false, LiveServer = false });
        var ids = nothing.SelectMany(section => section.Commands.Select(command => command.Id)).ToHashSet();

        Assert.HasCount(all.Count, everything);
        Assert.AreEqual(all.Sum(section => section.Commands.Count), everything.Sum(section => section.Commands.Count));
        Assert.IsFalse(nothing.Any(section => section.Id == "anticheat"));
        Assert.DoesNotContain("mix-ranked", ids);
        Assert.DoesNotContain("admin-rmix", ids);
        Assert.DoesNotContain("site-ranking", ids);
        Assert.DoesNotContain("site-skins", ids);
        Assert.DoesNotContain("admin-skins", ids);
        Assert.DoesNotContain("admin-anticheat", ids);
        Assert.Contains("mix-vote", ids);
        Assert.Contains("queue-slot", ids);
        Assert.IsTrue(nothing.All(section => section.Commands.Count > 0));
        Assert.AreEqual(all.Sum(section => section.Commands.Count), catalog.All(english).Sum(section => section.Commands.Count));
    }

    [TestMethod]
    public void TheSkinsNeedTheRankingAsWell()
    {
        using var factory = new HubWebFactory();

        var catalog = factory.Services.GetRequiredService<ICommandCatalog>();
        var sections = catalog.Sections(new CultureInfo("en-US"), new ModuleSettings { Ranking = false, SkinShop = true });

        Assert.IsFalse(sections.SelectMany(section => section.Commands).Any(command => command.Id == "site-skins"));
    }

    [TestMethod]
    public void BackticksBecomeCodeAndTheRestIsEncoded()
    {
        var html = CommandText.Format("Use `!mix <a>` & <b>wait</b>").ToString();

        Assert.AreEqual("Use <code>!mix &lt;a&gt;</code> &amp; &lt;b&gt;wait&lt;/b&gt;", html);
        Assert.AreEqual(string.Empty, CommandText.Format(null).ToString());
    }

    private static List<string> Signature(List<CommandSection> sections)
    {
        return
        [
            .. sections.Select(section => $"{section.Id}|{section.Icon}|{section.Module}|{section.Admin}"),
            .. sections.SelectMany(section => section.Commands.Select(command => string.Join('|', section.Id, command.Id, command.Module, command.Admin, string.IsNullOrEmpty(command.Title), command.Names.Count, command.Syntax.Count, command.Who.Count, command.When.Count, command.Examples.Count, command.Notes.Count)))
        ];
    }

    private static List<string> Names(List<CommandSection> sections)
    {
        return [.. sections.Where(section => section.Id != TypingSection).SelectMany(section => section.Commands.Select(command => $"{command.Id}:{string.Join(' ', command.Names)}"))];
    }

    private static List<CommandSection> Load(string culture)
    {
        var path = Path.Combine(WebProjectDirectory(), "Resources", "Commands", $"commands.{culture}.json");

        return JsonSerializer.Deserialize<CommandCatalogDocument>(File.ReadAllText(path), JsonOptions)?.Sections ?? [];
    }

    private static string WebProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, WebProject)))
            directory = directory.Parent;

        Assert.IsNotNull(directory, "The web project folder was not found.");

        return Path.Combine(directory.FullName, WebProject);
    }
}
