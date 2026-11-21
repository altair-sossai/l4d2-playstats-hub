using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Admin;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Admin;

public class AdministratorFormValidator : AbstractValidator<AdministratorForm>
{
    public AdministratorFormValidator()
    {
        RuleFor(form => form.CommunityId).AnySteamId();
    }
}
