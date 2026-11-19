using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class LiveDelay(IServiceScopeFactory scopeFactory) : ILiveDelay
{
    private readonly Lock _lock = new();
    private TimeSpan? _current;

    public TimeSpan Current
    {
        get
        {
            lock (_lock)
            {
                _current ??= Load();

                return _current.Value;
            }
        }
    }

    public void Set(int seconds)
    {
        lock (_lock)
        {
            _current = TimeSpan.FromSeconds(Math.Clamp(seconds, 0, AppConsts.LiveMaxDelayInSeconds));
        }
    }

    private TimeSpan Load()
    {
        using var scope = scopeFactory.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<ISettingsService>().GetAsync<LiveServerSettings>().GetAwaiter().GetResult();

        settings.Normalize();

        return TimeSpan.FromSeconds(settings.DelayInSeconds);
    }
}
