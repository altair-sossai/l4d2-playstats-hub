using FluentValidation;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Admin;

public class CampaignFormValidator : AbstractValidator<CampaignForm>
{
    public CampaignFormValidator()
    {
        RuleFor(form => form.Name).Required(AppConsts.CampaignRequiredMessage);
        RuleFor(form => form.Maps).Must(maps => CampaignMaps.Parse(maps).Length > 0).WithMessage(AppConsts.CampaignRequiredMessage);
    }
}
