using System.Net;
using System.Text;

namespace L4D2PlayStats.Hub.Tests.Support.Skins;

public class StubHttpMessageHandler(HttpStatusCode status, string content) : HttpMessageHandler
{
    public List<string> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request.RequestUri!.ToString());

        var response = new HttpResponseMessage(status) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

        return Task.FromResult(response);
    }
}