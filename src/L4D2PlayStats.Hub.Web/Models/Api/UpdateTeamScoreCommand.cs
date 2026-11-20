namespace L4D2PlayStats.Hub.Web.Models.Api;

public class UpdateTeamScoreCommand
{
    public char Team { get; set; }
    public int Score { get; set; }
}