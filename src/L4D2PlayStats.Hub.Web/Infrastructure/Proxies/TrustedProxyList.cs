using System.Net;
using IPNetwork = System.Net.IPNetwork;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Proxies;

public class TrustedProxyList
{
    private TrustedProxyList()
    {
    }

    public IReadOnlyList<IPAddress> Addresses { get; private init; } = [];
    public IReadOnlyList<IPNetwork> Networks { get; private init; } = [];
    public IReadOnlyList<string> Invalid { get; private init; } = [];

    public static TrustedProxyList Parse(string? value)
    {
        var addresses = new List<IPAddress>();
        var networks = new List<IPNetwork>();
        var invalid = new List<string>();

        foreach (var entry in (value ?? string.Empty).Split(AppConsts.TrustedProxySeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (entry.Contains('/'))
            {
                if (IPNetwork.TryParse(entry, out var network))
                    networks.Add(network);
                else
                    invalid.Add(entry);
            }
            else if (IPAddress.TryParse(entry, out var address))
                addresses.Add(address);
            else
                invalid.Add(entry);

        return new TrustedProxyList { Addresses = addresses, Networks = networks, Invalid = invalid };
    }

    public void ApplyTo(ForwardedHeadersOptions options)
    {
        foreach (var address in Addresses)
            options.KnownProxies.Add(address);

        foreach (var network in Networks)
            options.KnownIPNetworks.Add(network);
    }
}
