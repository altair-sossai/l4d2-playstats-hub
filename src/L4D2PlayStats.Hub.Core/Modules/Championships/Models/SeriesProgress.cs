namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record SeriesProgress(int WinsA, int WinsB, int Ties, int Played, bool Finished, int? WinnerTeamId, int RequiredGames)
{
    public bool Draw => Finished && WinnerTeamId == null;
}
