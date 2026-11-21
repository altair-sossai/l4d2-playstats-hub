namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IValidationService
{
    Task<string?> ErrorAsync<T>(T command, CancellationToken cancellationToken = default);
}
