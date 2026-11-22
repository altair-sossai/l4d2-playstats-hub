using System.Text.Json;
using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class PageConfigTests
{
    private const string Solution = "L4D2PlayStats.Hub.slnx";

    [TestMethod]
    public void EveryKeyAPageScriptReadsIsWrittenByItsView()
    {
        var web = Path.Combine(Root(), "src", "L4D2PlayStats.Hub.Web");
        var views = Directory.GetFiles(Path.Combine(web, "Views"), "*.cshtml", SearchOption.AllDirectories).Select(File.ReadAllText).ToList();
        var checkedScripts = 0;

        foreach (var file in Directory.GetFiles(Path.Combine(web, "wwwroot", "js", "pages"), "*.js"))
        {
            var script = File.ReadAllText(file);
            var head = Head().Match(script);

            if (!head.Success)
                continue;

            var variable = head.Groups[1].Value;
            var id = head.Groups[2].Value;
            var view = views.SingleOrDefault(text => text.Contains($"id=\"{id}\""));

            Assert.IsNotNull(view, $"{Path.GetFileName(file)}: no view writes {id}");

            var block = Regex.Match(view, $"id=\"{Regex.Escape(id)}\">(.*?)</script>", RegexOptions.Singleline).Groups[1].Value;
            var keys = Key().Matches(block).Select(match => Decode(match.Groups[1].Value)).ToHashSet();

            foreach (Match read in Regex.Matches(script, Regex.Escape(variable) + @"\.(?:values\.|texts\.)?(\w+)"))
            {
                var name = read.Groups[1].Value;

                if (name is not ("values" or "texts"))
                    Assert.Contains(name, keys, $"{Path.GetFileName(file)} reads {name}, which {id} does not have");
            }

            foreach (Match read in Regex.Matches(script, Regex.Escape(variable) + @"\.texts\[(""(?:[^""\\]|\\.)*"")\]"))
                Assert.Contains(JsonSerializer.Deserialize<string>(read.Groups[1].Value)!, keys, $"{Path.GetFileName(file)} reads the text {read.Groups[1].Value}, which {id} does not have");

            checkedScripts++;
        }

        Assert.IsGreaterThan(15, checkedScripts);
    }

    private static string Decode(string key)
    {
        return JsonSerializer.Deserialize<string>($"\"{key.Replace("@@", "@")}\"")!;
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !File.Exists(Path.Combine(directory.FullName, Solution)))
            directory = directory.Parent;

        Assert.IsNotNull(directory, $"{Solution} not found");

        return directory.FullName;
    }

    [GeneratedRegex(@"^var (\w+) = JSON\.parse\(document\.getElementById\(""([^""]+)""\)\.textContent\);")]
    private static partial Regex Head();

    [GeneratedRegex(@"""((?:[^""\\]|\\.)*)""\s*:")]
    private static partial Regex Key();
}
