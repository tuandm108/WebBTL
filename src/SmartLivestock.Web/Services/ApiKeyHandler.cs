namespace SmartLivestock.Web.Services;

public sealed class ApiKeyHandler(string key) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-SmartLivestock-Api-Key", key);
        return base.SendAsync(request, cancellationToken);
    }
}
