namespace L4D2PlayStats.Hub.Core.Modules.Patents.Settings;

public class PatentSettings
{
    public int MaxExperience { get; set; } = AppConsts.PatentMaxExperience;
    public double Curve { get; set; } = AppConsts.PatentCurve;

    public void Normalize()
    {
        MaxExperience = Math.Clamp(MaxExperience, AppConsts.PatentMaxExperienceMinimum, AppConsts.PatentMaxExperienceMaximum);
        Curve = double.IsFinite(Curve) ? Math.Round(Math.Clamp(Curve, AppConsts.PatentCurveMinimum, AppConsts.PatentCurveMaximum), AppConsts.PatentCurveDecimals) : AppConsts.PatentCurve;
    }
}
