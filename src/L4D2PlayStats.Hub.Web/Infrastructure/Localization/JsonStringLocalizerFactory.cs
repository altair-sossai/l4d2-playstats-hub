using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Localization;

public class JsonStringLocalizerFactory(IWebHostEnvironment environment) : IStringLocalizerFactory
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cultures = new();

    public IStringLocalizer Create(Type resourceSource)
    {
        return new JsonStringLocalizer(this);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new JsonStringLocalizer(this);
    }

    public string? Find(string name, CultureInfo culture)
    {
        var current = culture;

        while (!string.IsNullOrEmpty(current.Name))
        {
            if (Load(current.Name).TryGetValue(name, out var value))
                return value;

            current = current.Parent;
        }

        return null;
    }

    public IReadOnlyDictionary<string, string> Load(string culture)
    {
        return _cultures.GetOrAdd(culture, name =>
        {
            var folder = Path.Combine(environment.ContentRootPath, AppConsts.ResourcesFolder);
            var texts = new Dictionary<string, string>();

            if (!Directory.Exists(folder))
                return texts;

            var files = Directory.EnumerateFiles(folder, $"{name}.json").Concat(Directory.EnumerateFiles(folder, $"{name}.*.json").Order());

            foreach (var file in files)
            foreach (var pair in JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) ?? [])
                texts[pair.Key] = pair.Value;

            return texts;
        });
    }
}
