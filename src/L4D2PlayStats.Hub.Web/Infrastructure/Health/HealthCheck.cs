using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Health;

public static class HealthCheck
{
    public static bool IsCommand(string[] args)
    {
        return args.Length > 0 && args[0] == AppConsts.HealthCommand;
    }

    public static async Task<int> RunAsync(string[] args)
    {
        var address = args.Length > 1 ? args[1] : LocalAddress(Environment.GetEnvironmentVariable(AppConsts.UrlsVariable), Environment.GetEnvironmentVariable(AppConsts.HttpPortsVariable));

        using var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(AppConsts.HealthTimeoutInSeconds);

        try
        {
            using var response = await client.GetAsync($"{address.TrimEnd('/')}{AppConsts.HealthPath}");

            Console.WriteLine($"{(int)response.StatusCode} {address}{AppConsts.HealthPath}");

            return response.IsSuccessStatusCode ? 0 : 1;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"{address}{AppConsts.HealthPath}: {exception.Message}");

            return 1;
        }
    }

    public static string LocalAddress(string? urls, string? httpPorts)
    {
        var entries = (urls ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var url = entries.FirstOrDefault(entry => entry.StartsWith(Uri.UriSchemeHttp + "://", StringComparison.OrdinalIgnoreCase)) ?? entries.FirstOrDefault();

        if (url != null)
            return Local(url);

        var port = (httpPorts ?? string.Empty).Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();

        return $"{Uri.UriSchemeHttp}://{AppConsts.HealthHost}:{port ?? AppConsts.DefaultHttpPort}";
    }

    private static string Local(string url)
    {
        var match = Regex.Match(url, AppConsts.ListenUrlPattern);

        if (!match.Success)
            return url;

        var host = match.Groups["host"].Value;
        var port = match.Groups["port"].Value;

        if (AppConsts.WildcardHosts.Contains(host))
            host = AppConsts.HealthHost;

        return string.IsNullOrEmpty(port) ? $"{match.Groups["scheme"].Value}://{host}" : $"{match.Groups["scheme"].Value}://{host}:{port}";
    }
}
