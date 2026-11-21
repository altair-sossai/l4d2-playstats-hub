using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Players;

public class PunishmentCommandValidator : AbstractValidator<PunishmentCommand>
{
    public PunishmentCommandValidator()
    {
        RuleFor(command => command.CommunityId).SteamId();
        RuleFor(command => command.LostExperiencePoints).GreaterThan(0).WithMessage(AppConsts.PunishmentPointsInvalidMessage);
    }
}
