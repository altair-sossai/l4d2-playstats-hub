using System.Net;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Infrastructure.Proxies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class TrustedProxiesTests
{
    private const string Visitor = "198.51.100.9";

    [TestMethod]
    public void TheListAcceptsAddressesAndNetworksSeparatedByCommas()
    {
        var list = TrustedProxyList.Parse(" 10.0.0.5, 172.16.0.0/12;fd00::/8  ::1 ");

        CollectionAssert.AreEqual(new[] { IPAddress.Parse("10.0.0.5"), IPAddress.Parse("::1") }, list.Addresses.ToArray());
        CollectionAssert.AreEqual(new[] { IPNetwork.Parse("172.16.0.0/12"), IPNetwork.Parse("fd00::/8") }, list.Networks.ToArray());
        Assert.IsEmpty(list.Invalid);
    }

    [TestMethod]
    public void TheListKeepsWhatItCouldNotRead()
    {
        var list = TrustedProxyList.Parse("proxy.local, 10.0.0.300, 10.0.0.0/40, 192.168.1.1");

        CollectionAssert.AreEqual(new[] { "proxy.local", "10.0.0.300", "10.0.0.0/40" }, list.Invalid.ToArray());
        CollectionAssert.AreEqual(new[] { IPAddress.Parse("192.168.1.1") }, list.Addresses.ToArray());
    }

    [TestMethod]
    public void AnEmptySettingTrustsNothingMore()
    {
        var list = TrustedProxyList.Parse(null);

        Assert.IsEmpty(list.Addresses);
        Assert.IsEmpty(list.Networks);
        Assert.IsEmpty(list.Invalid);
    }

    [TestMethod]
    public async Task ForwardedHeadersFromAnUnknownAddressAreIgnored()
    {
        using var factory = new HubWebFactory();

        var context = await SendAsync(factory, "203.0.113.7");

        Assert.AreEqual("203.0.113.7", context.Connection.RemoteIpAddress!.ToString());
        Assert.AreEqual("http", context.Request.Scheme);
        Assert.AreNotEqual("evil.example", context.Request.Host.Host);
    }

    [TestMethod]
    public async Task ForwardedHeadersFromTheLoopbackProxyAreUsed()
    {
        using var factory = new HubWebFactory();

        var ipv4 = await SendAsync(factory, "127.0.0.1");
        var ipv6 = await SendAsync(factory, "::1");
        var mapped = await SendAsync(factory, "::ffff:127.0.0.1");

        foreach (var context in new[] { ipv4, ipv6, mapped })
        {
            Assert.AreEqual(Visitor, context.Connection.RemoteIpAddress!.ToString());
            Assert.AreEqual("https", context.Request.Scheme);
            Assert.AreEqual("evil.example", context.Request.Host.Host);
        }
    }

    [TestMethod]
    public async Task TheAdministratorCanTrustAProxyInAnotherNetwork()
    {
        using var factory = new HubWebFactory();
        using var configured = factory.WithWebHostBuilder(builder => builder.UseSetting("Hub:TrustedProxies", "172.18.0.0/16, 10.1.2.3"));

        var network = await SendAsync(configured, "172.18.0.4");
        var address = await SendAsync(configured, "10.1.2.3");
        var mapped = await SendAsync(configured, "::ffff:172.18.0.9");
        var other = await SendAsync(configured, "10.1.2.4");

        Assert.AreEqual(Visitor, network.Connection.RemoteIpAddress!.ToString());
        Assert.AreEqual("https", network.Request.Scheme);
        Assert.AreEqual(Visitor, address.Connection.RemoteIpAddress!.ToString());
        Assert.AreEqual(Visitor, mapped.Connection.RemoteIpAddress!.ToString());
        Assert.AreEqual("10.1.2.4", other.Connection.RemoteIpAddress!.ToString());
        Assert.AreEqual("http", other.Request.Scheme);
    }

    private static Task<HttpContext> SendAsync(WebApplicationFactory<Program> factory, string remoteAddress)
    {
        return factory.Server.SendAsync(context =>
        {
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/api/ping";
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteAddress);
            context.Request.Headers["X-Forwarded-For"] = Visitor;
            context.Request.Headers["X-Forwarded-Proto"] = "https";
            context.Request.Headers["X-Forwarded-Host"] = "evil.example";
        });
    }
}