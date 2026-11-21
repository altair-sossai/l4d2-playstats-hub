using FluentValidation;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.AntiCheat;

public class AntiCheatAccessModeCommandValidator : AbstractValidator<AntiCheatAccessModeCommand>
{
    public AntiCheatAccessModeCommandValidator()
    {
        RuleFor(command => command.Mode).Must(mode => AntiCheatAccessNames.TryParseMode(mode, out _)).WithMessage(AppConsts.AntiCheatModeInvalidMessage);
    }
}
