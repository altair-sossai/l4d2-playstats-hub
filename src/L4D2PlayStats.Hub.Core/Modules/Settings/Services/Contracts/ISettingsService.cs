namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

public interface ISettingsService
{
    Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : class, new();
    Task SaveAsync<T>(T settings, CancellationToken cancellationToken = default) where T : class, new();
}