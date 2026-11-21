using FluentValidation;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Skins;

public class SkinShopSettingsValidator : AbstractValidator<SkinShopSettings>
{
    public SkinShopSettingsValidator()
    {
        RuleFor(settings => settings.AssetsUrl).HttpUrl(AppConsts.SkinSettingsInvalidUrl);
        RuleFor(settings => settings.CatalogUrl).HttpUrl(AppConsts.SkinSettingsInvalidUrl);
        RuleFor(settings => settings.AuthorContactUrl).HttpUrl(AppConsts.SkinSettingsInvalidUrl);
        RuleFor(settings => settings.AuthorRequestUrl).HttpUrl(AppConsts.SkinSettingsInvalidUrl);
    }
}
