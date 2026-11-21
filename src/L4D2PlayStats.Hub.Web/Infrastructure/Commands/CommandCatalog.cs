using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Infrastructure.Commands.Contracts;
using L4D2PlayStats.Hub.Web.Models.Commands;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Commands;

public class CommandCatalog(IWebHostEnvironment environment) : ICommandCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly ConcurrentDictionary<string, IReadOnlyList<CommandSection>> _cultures = new();

    public IReadOnlyList<CommandSection> All(CultureInfo culture)
    {
        return _cultures.GetOrAdd(culture.Name, _ => Load(FileCulture(culture)));
    }

    public List<CommandSection> Sections(CultureInfo culture, ModuleSettings modules)
    {
        return
        [
            .. All(culture)
                .Where(section => IsEnabled(section.Module, modules))
                .Select(section => section with { Commands = [.. section.Commands.Where(command => IsEnabled(command.Module, modules))] })
                .Where(section => section.Commands.Count > 0)
        ];
    }

    private static bool IsEnabled(HubModule? module, ModuleSettings modules)
    {
        return module == null || modules.IsEnabled(module.Value);
    }

    private string FileCulture(CultureInfo culture)
    {
        var current = culture;

        while (!string.IsNullOrEmpty(current.Name))
        {
            if (File.Exists(FilePath(current.Name)))
                return current.Name;

            current = current.Parent;
        }

        return AppConsts.CommandsDefaultCulture;
    }

    private IReadOnlyList<CommandSection> Load(string culture)
    {
        var path = FilePath(culture);

        if (!File.Exists(path))
            return [];

        var document = JsonSerializer.Deserialize<CommandCatalogDocument>(File.ReadAllText(path), JsonOptions);

        return document?.Sections ?? [];
    }

    private string FilePath(string culture)
    {
        return Path.Combine(environment.ContentRootPath, AppConsts.ResourcesFolder, AppConsts.CommandsFolder, string.Format(CultureInfo.InvariantCulture, AppConsts.CommandsFilePattern, culture));
    }
}
