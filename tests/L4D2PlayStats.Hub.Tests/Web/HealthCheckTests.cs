using System.Net;
using System.Net.Sockets;
using System.Text;
using L4D2PlayStats.Hub.Web.Infrastructure.Health;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class HealthCheckTests
{
    [TestMethod]
    [DataRow("http://+:8080", null, "http://127.0.0.1:8080")]
    [DataRow("http://0.0.0.0:5080", null, "http://127.0.0.1:5080")]
    [DataRow("http://*:5080;https://*:5443", null, "http://127.0.0.1:5080")]
    [DataRow("https://+:5443;http://[::]:5081", null, "http://127.0.0.1:5081")]
    [DataRow("http://localhost:5111", null, "http://localhost:5111")]
    [DataRow("http://[::1]:5111", null, "http://[::1]:5111")]
    [DataRow(null, "8080", "http://127.0.0.1:8080")]
    [DataRow("", "8081;8082", "http://127.0.0.1:8081")]
    [DataRow(null, null, "http://127.0.0.1:5000")]
    public void TheCheckCallsTheAddressTheSiteListensOn(string? urls, string? httpPorts, string expected)
    {
        Assert.AreEqual(expected, HealthCheck.LocalAddress(urls, httpPorts));
    }

    [TestMethod]
    public void OnlyTheHealthArgumentRunsTheCheck()
    {
        Assert.IsTrue(HealthCheck.IsCommand(["health"]));
        Assert.IsFalse(HealthCheck.IsCommand(["admin", "list"]));
        Assert.IsFalse(HealthCheck.IsCommand([]));
    }

    [TestMethod]
    public async Task TheCheckSucceedsWhenThePingAnswers()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);

        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = AnswerAsync(listener, "HTTP/1.1 200 OK");
        var code = await HealthCheck.RunAsync(["health", $"http://127.0.0.1:{port}"]);
        var request = await server;

        Assert.AreEqual(0, code);
        Assert.StartsWith("GET /api/ping HTTP/1.1", request);
    }

    [TestMethod]
    public async Task TheCheckFailsWhenThePingFails()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);

        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = AnswerAsync(listener, "HTTP/1.1 503 Service Unavailable");

        Assert.AreEqual(1, await HealthCheck.RunAsync(["health", $"http://127.0.0.1:{port}"]));

        await server;
    }

    [TestMethod]
    public async Task TheCheckFailsWhenNothingListens()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);

        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        listener.Stop();

        Assert.AreEqual(1, await HealthCheck.RunAsync(["health", $"http://127.0.0.1:{port}"]));
    }

    private static async Task<string> AnswerAsync(TcpListener listener, string statusLine)
    {
        using var client = await listener.AcceptTcpClientAsync();
        await using var stream = client.GetStream();

        var buffer = new byte[4096];
        var read = await stream.ReadAsync(buffer);
        var response = Encoding.ASCII.GetBytes($"{statusLine}\r\nContent-Type: application/json\r\nContent-Length: 2\r\nConnection: close\r\n\r\n{{}}");

        await stream.WriteAsync(response);

        return Encoding.ASCII.GetString(buffer, 0, read);
    }
}