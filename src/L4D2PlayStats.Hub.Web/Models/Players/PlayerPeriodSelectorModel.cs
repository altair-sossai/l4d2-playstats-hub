namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerPeriodSelectorModel(PlayerPeriodModel period, Func<int?, string> url)
{
    public PlayerPeriodModel Period { get; } = period;
    public Func<int?, string> Url { get; } = url;
}
