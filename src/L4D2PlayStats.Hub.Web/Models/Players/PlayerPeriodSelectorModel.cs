namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerPeriodSelectorModel(PlayerPeriodModel period, Func<string?, string> url)
{
    public PlayerPeriodModel Period { get; } = period;
    public Func<string?, string> Url { get; } = url;
}