using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Instrumentation.AspNetCore;

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

        services.Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter = context => IsTracked(context.Request.Path));

        return services;
    }

    public static bool IsTracked(PathString path)
    {
        var value = path.Value ?? string.Empty;

        if (AppConsts.TelemetryIgnoredPrefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            return false;

        return !AppConsts.TelemetryIgnoredSegments.Any(segment => value.Contains(segment, StringComparison.OrdinalIgnoreCase));
    }
}
