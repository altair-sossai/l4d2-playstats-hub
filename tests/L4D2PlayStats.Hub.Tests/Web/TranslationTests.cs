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
        var translations = Load(Path.Combine(web, "Resources", $"{culture}.json"));

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
        var portuguese = Load(Path.Combine(web, "Resources", "pt-BR.json")).Keys.ToHashSet();
        var spanish = Load(Path.Combine(web, "Resources", "es-ES.json")).Keys.ToHashSet();

        Assert.IsTrue(portuguese.SetEquals(spanish), $"Only in one language: {string.Join(" | ", portuguese.Except(spanish).Concat(spanish.Except(portuguese)))}");
    }

    [TestMethod]
    [DataRow("pt-BR")]
    [DataRow("es-ES")]
    public void NoTranslationIsEmpty(string culture)
    {
        var translations = Load(Path.Combine(WebProjectDirectory(), "Resources", $"{culture}.json"));
        var empty = translations.Where(pair => string.IsNullOrWhiteSpace(pair.Value)).Select(pair => pair.Key).ToList();

        Assert.IsEmpty(empty);
    }

    private static Dictionary<string, string> Load(string path)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? [];
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
