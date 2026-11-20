namespace L4D2PlayStats.Hub.Tests.Support;

public class RoutingHttpMessageHandler(Func<Uri, HttpResponseMessage> route) : HttpMessageHandler
{
    public List<Uri> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        lock (Requests)
        {
            Requests.Add(request.RequestUri!);
        }

        return Task.FromResult(route(request.RequestUri!));
    }
}