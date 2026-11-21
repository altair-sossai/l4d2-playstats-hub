using FluentValidation;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class ValidationService(IServiceProvider serviceProvider) : IValidationService
{
    public async Task<string?> ErrorAsync<T>(T command, CancellationToken cancellationToken = default)
    {
        var validator = serviceProvider.GetService<IValidator<T>>();

        if (validator == null)
            return null;

        var result = await validator.ValidateAsync(command, cancellationToken);

        return result.IsValid ? null : result.Errors[0].ErrorMessage;
    }
}
