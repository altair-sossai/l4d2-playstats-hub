namespace L4D2PlayStats.Hub.Core.Modules.Seasons;

public class Season
{
    public int Id { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public bool Contains(DateTime date)
    {
        return date >= StartsAt && date < EndsAt;
    }
}