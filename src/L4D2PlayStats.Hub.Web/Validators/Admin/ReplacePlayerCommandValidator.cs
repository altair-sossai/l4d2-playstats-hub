using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Api;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Admin;

public class ReplacePlayerCommandValidator : AbstractValidator<ReplacePlayerCommand>
{
    public ReplacePlayerCommandValidator()
    {
        RuleFor(command => command.Half).Must(half => half >= 0).WithMessage(AppConsts.ReplacementPlaceRequiredMessage);
        RuleFor(command => command.Survivor).NotNull().WithMessage(AppConsts.ReplacementPlaceRequiredMessage);
        RuleFor(command => command.Replaced).SteamId(AppConsts.ReplacedPlayerRequiredMessage);
        RuleFor(command => command.Replacement).SteamId(AppConsts.ReplacementPlayerRequiredMessage);
    }
}
