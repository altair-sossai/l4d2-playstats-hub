namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityStrength(double Rating, double Deviation)
{
    public double Conservative => Rating - AppConsts.CommunityConservativeDeviations * Deviation;
}
