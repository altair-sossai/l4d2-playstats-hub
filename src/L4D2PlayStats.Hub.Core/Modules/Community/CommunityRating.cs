namespace L4D2PlayStats.Hub.Core.Modules.Community;

public class CommunityRating
{
    public long CommunityId { get; set; }
    public double Rating { get; set; }
    public int Comparisons { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
}