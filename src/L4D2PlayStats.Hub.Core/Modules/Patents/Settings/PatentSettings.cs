namespace L4D2PlayStats.Hub.Core.Modules.Patents.Settings;

public class PatentSettings
{
    public int MaxExperience { get; set; } = AppConsts.PatentMaxExperience;

    public void Normalize()
    {
        MaxExperience = Math.Clamp(MaxExperience, AppConsts.PatentMaxExperienceMinimum, AppConsts.PatentMaxExperienceMaximum);
    }
}
