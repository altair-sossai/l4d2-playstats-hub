using FluentValidation;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Models.Bans;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Bans;

public class BanFormValidator : AbstractValidator<BanForm>
{
    public BanFormValidator()
    {
        RuleFor(form => form.CommunityId).AnySteamId();
        RuleFor(form => form.Duration).Must(BanDurations.IsValid).WithMessage(AppConsts.BanInvalidEndMessage);
    }
}
