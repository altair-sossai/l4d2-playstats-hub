namespace L4D2PlayStats.Hub.Web.Models.Commands;

public class CommandsViewModel
{
    public List<CommandSection> Sections { get; init; } = [];

    public IEnumerable<CommandSection> PlayerSections => Sections.Where(section => !section.Admin);

    public IEnumerable<CommandSection> AdminSections => Sections.Where(section => section.Admin);

    public int PlayerCommands => PlayerSections.Sum(section => section.Commands.Count);
}
