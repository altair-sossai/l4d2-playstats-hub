using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Skins;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Skins;

public class SkinOwnershipCommandValidator : AbstractValidator<SkinOwnershipCommand>
{
    public SkinOwnershipCommandValidator()
    {
        RuleFor(command => command.CommunityId).SteamId();
        RuleFor(command => command.SkinId).Required(AppConsts.SkinRequiredMessage);
    }
}
