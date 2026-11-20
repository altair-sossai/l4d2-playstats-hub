namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

public interface ISettingsEncryptionService
{
    Task<int> EncryptPlainSecretsAsync(CancellationToken cancellationToken = default);
}