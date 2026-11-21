using L4D2PlayStats.Hub.Core.Modules.Patents.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Patents.Helpers;

public static class PatentCatalog
{
    public static readonly IReadOnlyList<string> Names =
    [
        "Bronze #1",
        "Bronze #2",
        "Bronze #3",
        "Bronze #4",
        "Bronze #5",
        "Silver #1",
        "Silver #2",
        "Silver #3",
        "Silver #4",
        "Silver #5",
        "Gold #1",
        "Gold #2",
        "Gold #3",
        "Gold #4",
        "Global"
    ];

    public static PatentScale Default { get; } = Scale(AppConsts.PatentMaxExperience, AppConsts.PatentCurve);

    public static PatentScale Scale(int maxExperience, double curve)
    {
        return new PatentScale(maxExperience, curve, Names);
    }
}
