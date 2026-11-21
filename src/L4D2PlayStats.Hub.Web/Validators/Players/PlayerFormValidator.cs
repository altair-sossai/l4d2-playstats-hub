using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Players;

public class PlayerFormValidator : AbstractValidator<PlayerForm>
{
    public PlayerFormValidator()
    {
        RuleFor(form => form.CommunityId).AnySteamId();
    }
}
