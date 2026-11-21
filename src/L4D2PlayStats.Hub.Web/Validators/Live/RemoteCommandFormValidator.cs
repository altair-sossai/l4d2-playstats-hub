using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Live;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Live;

public class RemoteCommandFormValidator : AbstractValidator<RemoteCommandForm>
{
    public RemoteCommandFormValidator()
    {
        RuleFor(form => form.Command).Required(AppConsts.InvalidRemoteCommandMessage);
    }
}
