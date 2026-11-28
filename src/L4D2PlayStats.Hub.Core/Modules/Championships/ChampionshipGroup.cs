namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipGroup
{
    public int Id { get; set; }
    public int StageId { get; set; }
    public string Name { get; set; } = null!;
    public int Order { get; set; }
    public List<ChampionshipGroupTeam> Teams { get; set; } = [];
}
