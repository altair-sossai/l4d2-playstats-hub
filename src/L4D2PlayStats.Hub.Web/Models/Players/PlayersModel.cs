namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayersModel(PagedListModel<PlayerListRow> players, PlayerResultProperty orderBy, bool asc, PlayerPeriodModel period)
{
    public PagedListModel<PlayerListRow> Players { get; } = players;
    public PlayerResultProperty OrderBy { get; } = orderBy;
    public bool Asc { get; } = asc;
    public PlayerPeriodModel Period { get; } = period;
}
