using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Infrastructure;

public class SharedResource;

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
            var path = Path.Combine(environment.ContentRootPath, AppConsts.ResourcesFolder, $"{name}.json");

            if (!File.Exists(path))
                return new Dictionary<string, string>();

            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? [];
        });
    }
}

public class JsonStringLocalizer(JsonStringLocalizerFactory factory) : IStringLocalizer
{
    public LocalizedString this[string name]
    {
        get
        {
            var value = factory.Find(name, CultureInfo.CurrentUICulture);

            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = factory.Find(name, CultureInfo.CurrentUICulture);

            return new LocalizedString(name, string.Format(CultureInfo.CurrentCulture, value ?? name, arguments), value == null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return factory.Load(CultureInfo.CurrentUICulture.Name).Select(pair => new LocalizedString(pair.Key, pair.Value));
    }
}
