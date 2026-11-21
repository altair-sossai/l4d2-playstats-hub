using FluentValidation;
using L4D2PlayStats.Hub.Web.Models.Skins;
using L4D2PlayStats.Hub.Web.Validators.Extensions;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Validators.Skins;

public class SkinPriceCommandValidator : AbstractValidator<SkinPriceCommand>
{
    public SkinPriceCommandValidator()
    {
        RuleFor(command => command.Price).Range(CoreConsts.SkinMinPrice, CoreConsts.SkinMaxPrice, AppConsts.SkinPriceInvalidMessage);
    }
}
