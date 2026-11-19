using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class LiveSettings(IServiceScopeFactory scopeFactory) : ILiveSettings
{
    private readonly Lock _lock = new();
    private LiveServerSettings? _current;

    public LiveServerSettings Current
    {
        get
        {
            lock (_lock)
            {
                _current ??= Load();

                return _current;
            }
        }
    }

    public void Set(LiveServerSettings settings)
    {
        settings.Normalize();

        lock (_lock)
        {
            _current = settings;
        }
    }

    private LiveServerSettings Load()
    {
        using var scope = scopeFactory.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<ISettingsService>().GetAsync<LiveServerSettings>().GetAwaiter().GetResult();

        settings.Normalize();

        return settings;
    }
}
