using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Telemetry;

public static class HubTelemetry
{
    public static bool IsConfigured(IConfiguration configuration)
    {
        return !string.IsNullOrWhiteSpace(configuration[AppConsts.ApplicationInsightsConnectionStringKey]);
    }

    public static IServiceCollection AddHubTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        if (!IsConfigured(configuration))
            return services;

        services.AddOpenTelemetry().UseAzureMonitor(options =>
        {
            options.ConnectionString = configuration[AppConsts.ApplicationInsightsConnectionStringKey];
            options.SamplingRatio = AppConsts.TelemetrySamplingRatio;
        });

        services.Configure<LoggerFilterOptions>(options => options.AddFilter<OpenTelemetryLoggerProvider>(null, AppConsts.TelemetryMinimumLogLevel));
        services.Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter = context => IsTracked(context.Request));

        return services;
    }

    public static bool IsTracked(HttpRequest request)
    {
        return IsTracked(request.Path) && !IsUnreferredComparison(request);
    }

    public static bool IsTracked(PathString path)
    {
        var value = path.Value ?? string.Empty;

        if (AppConsts.TelemetryIgnoredPaths.Any(ignored => string.Equals(value, ignored, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (value.StartsWith(AppConsts.TelemetryLivePrefix, StringComparison.OrdinalIgnoreCase) && AppConsts.TelemetryLivePollingSuffixes.Any(suffix => value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (AppConsts.TelemetryIgnoredPrefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            return false;

        return !AppConsts.TelemetryIgnoredSegments.Any(segment => value.Contains(segment, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUnreferredComparison(HttpRequest request)
    {
        return string.IsNullOrEmpty(request.Headers.Referer) && AppConsts.TelemetryComparisonPath.IsMatch(request.Path.Value ?? string.Empty);
    }
}
