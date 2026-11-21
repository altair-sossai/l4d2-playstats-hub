using FluentValidation;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Admin;

public class GameModeRuleValidator : AbstractValidator<GameModeRule>
{
    public GameModeRuleValidator()
    {
        RuleFor(rule => rule.Name).Required();
    }
}
