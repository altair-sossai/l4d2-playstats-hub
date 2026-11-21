using FluentValidation;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Web.Validators.Extensions;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string?> SteamId<T>(this IRuleBuilder<T, string?> rule, string requiredMessage = AppConsts.SteamIdRequiredMessage, string invalidMessage = AppConsts.SteamIdInvalidMessage)
    {
        return rule
            .NotEmpty().WithMessage(requiredMessage)
            .Must(value => string.IsNullOrWhiteSpace(value) || SteamIdParser.Parse(value) != null).WithMessage(invalidMessage);
    }

    public static IRuleBuilderOptions<T, string?> AnySteamId<T>(this IRuleBuilder<T, string?> rule, string message = AppConsts.InvalidSteamIdMessage)
    {
        return rule.SteamId(message, message);
    }

    public static IRuleBuilderOptions<T, string?> Required<T>(this IRuleBuilder<T, string?> rule, string message = AppConsts.NameRequiredMessage)
    {
        return rule.Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage(message);
    }

    public static IRuleBuilderOptions<T, string?> HttpUrl<T>(this IRuleBuilder<T, string?> rule, string message)
    {
        return rule.Must(value => value == null || SafeUrl.IsHttp(value)).WithMessage(message);
    }

    public static IRuleBuilderOptions<T, int?> Range<T>(this IRuleBuilder<T, int?> rule, int minimum, int maximum, string message)
    {
        return rule.Must(value => value == null || (value >= minimum && value <= maximum)).WithMessage(message);
    }
}
