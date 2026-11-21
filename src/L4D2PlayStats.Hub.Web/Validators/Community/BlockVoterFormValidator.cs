using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Community;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Community;

public class BlockVoterFormValidator : AbstractValidator<BlockVoterForm>
{
    public BlockVoterFormValidator()
    {
        RuleFor(form => form.CommunityId).AnySteamId();
    }
}
