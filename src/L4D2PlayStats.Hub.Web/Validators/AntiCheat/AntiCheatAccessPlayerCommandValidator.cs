using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.AntiCheat;

public class AntiCheatAccessPlayerCommandValidator : AbstractValidator<AntiCheatAccessPlayerCommand>
{
    public AntiCheatAccessPlayerCommandValidator()
    {
        RuleFor(command => command.Player).SteamId();
    }
}
