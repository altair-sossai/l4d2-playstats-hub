using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Players;

public class ForcedNameFormValidator : AbstractValidator<ForcedNameForm>
{
    public ForcedNameFormValidator()
    {
        RuleFor(form => form.CommunityId).AnySteamId();
        RuleFor(form => form.Name).Required(AppConsts.ForcedNameInvalidNameMessage);
    }
}
