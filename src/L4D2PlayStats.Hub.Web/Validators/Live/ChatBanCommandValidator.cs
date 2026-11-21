using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Live;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Live;

public class ChatBanCommandValidator : AbstractValidator<ChatBanCommand>
{
    public ChatBanCommandValidator()
    {
        RuleFor(command => command.CommunityId).SteamId();
    }
}
