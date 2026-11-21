using FluentValidation;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Validators.Extensions;

namespace L4D2PlayStats.Hub.Web.Validators.Admin;

public class ServerFormValidator : AbstractValidator<ServerForm>
{
    public ServerFormValidator()
    {
        RuleFor(form => form.Name).Required();
    }
}
