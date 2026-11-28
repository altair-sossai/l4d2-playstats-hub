using FluentValidation;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Models.Bans;

namespace L4D2PlayStats.Hub.Web.Validators.Bans;

public class BanEditFormValidator : AbstractValidator<BanEditForm>
{
    public BanEditFormValidator()
    {
        RuleFor(form => form.Duration).Must(BanDurations.IsValid).WithMessage(AppConsts.BanInvalidEndMessage);
    }
}
