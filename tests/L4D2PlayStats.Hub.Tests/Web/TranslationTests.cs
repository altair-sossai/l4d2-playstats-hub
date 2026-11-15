using System.Text.Json;
using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class TranslationTests
{
    private const string WebProject = "src/L4D2PlayStats.Hub.Web";

    [TestMethod]
    [DataRow("pt-BR")]
    [DataRow("es-ES")]
    public void EveryTextInTheViewsIsTranslated(string culture)
    {
        var web = WebProjectDirectory();
        var translations = Load(web, culture);

        var missing = Directory.EnumerateFiles(Path.Combine(web, "Views"), "*.cshtml", SearchOption.AllDirectories)
            .SelectMany(file => LocalizedText().Matches(File.ReadAllText(file)).Select(match => Regex.Unescape(match.Groups[1].Value)))
            .Distinct()
            .Where(key => !translations.ContainsKey(key))
            .OrderBy(key => key)
            .ToList();

        Assert.IsEmpty(missing, $"Texts without a {culture} translation: {string.Join(" | ", missing)}");
    }

    [TestMethod]
    public void BothLanguagesHaveTheSameTexts()
    {
        var web = WebProjectDirectory();
        var portuguese = Load(web, "pt-BR").Keys.ToHashSet();
        var spanish = Load(web, "es-ES").Keys.ToHashSet();

        Assert.IsTrue(portuguese.SetEquals(spanish), $"Only in one language: {string.Join(" | ", portuguese.Except(spanish).Concat(spanish.Except(portuguese)))}");
    }

    [TestMethod]
    [DataRow("pt-BR")]
    [DataRow("es-ES")]
    public void NoTranslationIsEmpty(string culture)
    {
        var translations = Load(WebProjectDirectory(), culture);
        var empty = translations.Where(pair => string.IsNullOrWhiteSpace(pair.Value)).Select(pair => pair.Key).ToList();

        Assert.IsEmpty(empty);
    }

    private static Dictionary<string, string> Load(string web, string culture)
    {
        var folder = Path.Combine(web, "Resources");
        var texts = new Dictionary<string, string>();

        foreach (var file in Directory.EnumerateFiles(folder, $"{culture}.json").Concat(Directory.EnumerateFiles(folder, $"{culture}.*.json").Order()))
            foreach (var pair in JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) ?? [])
                texts[pair.Key] = pair.Value;

        return texts;
    }

    private static string WebProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, WebProject)))
            directory = directory.Parent;

        Assert.IsNotNull(directory, "The web project folder was not found.");

        return Path.Combine(directory.FullName, WebProject);
    }

    [GeneratedRegex("""L\["((?:[^"\\]|\\.)*)"\s*[\],]""")]
    private static partial Regex LocalizedText();
}
