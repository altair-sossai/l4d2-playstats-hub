using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Models.Commands;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Commands.Contracts;

public interface ICommandCatalog
{
    IReadOnlyList<CommandSection> All(CultureInfo culture);

    List<CommandSection> Sections(CultureInfo culture, ModuleSettings modules);
}
