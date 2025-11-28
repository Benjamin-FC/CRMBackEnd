using System.Net.Http.Headers;

namespace CRMBackEnd.Infrastructure.Handlers;

/// <summary>
/// DelegatingHandler to inject Bearer token into HTTP requests
/// </summary>
public class BearerTokenHandler : DelegatingHandler
{
    private readonly string _token;

    public BearerTokenHandler(string token)
    {
        _token = token ?? throw new ArgumentNullException(nameof(token));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add Bearer token to Authorization header
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        return await base.SendAsync(request, cancellationToken);
    }
}
