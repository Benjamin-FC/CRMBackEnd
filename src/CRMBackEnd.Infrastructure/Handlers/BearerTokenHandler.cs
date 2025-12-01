using System.Net.Http.Headers;
using CRMBackEnd.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRMBackEnd.Infrastructure.Handlers;

/// <summary>
/// DelegatingHandler to inject Bearer token into HTTP requests
/// Can use either static token or dynamic token provider
/// </summary>
public class BearerTokenHandler : DelegatingHandler
{
    private readonly CRMEnvironmentSettings _settings;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BearerTokenHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public BearerTokenHandler(
        CRMEnvironmentSettings settings,
        IConfiguration configuration,
        ILogger<BearerTokenHandler> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings;
        _configuration = configuration;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string token;
        
        var useDevelopment = _configuration.GetValue<bool>("UseDevelopmentCRM");

        if (!useDevelopment)
        {
            // Production mode: Use dynamic token provider
            try
            {
                // Create a scope to resolve the service
                using var scope = _serviceProvider.CreateScope();
                var tokenService = scope.ServiceProvider.GetService(
                    Type.GetType("CRMBackEnd.API.Authentication.CrmTokenService, CRMBackEnd.API")!);
                
                if (tokenService != null)
                {
                    // Use reflection to call GetTokenAsync
                    var method = tokenService.GetType().GetMethod("GetTokenAsync");
                    if (method != null)
                    {
                        var task = (Task<string>)method.Invoke(tokenService, null)!;
                        token = await task;
                        _logger.LogDebug("Using dynamically generated bearer token for external CRM request");
                    }
                    else
                    {
                        token = _settings.BearerToken;
                        _logger.LogWarning("GetTokenAsync method not found, using static bearer token");
                    }
                }
                else
                {
                    token = _settings.BearerToken;
                    _logger.LogWarning("CrmTokenService not available, using static bearer token");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dynamic token, falling back to static token");
                token = _settings.BearerToken;
            }
        }
        else
        {
            // Development mode: Use static token
            token = _settings.BearerToken;
            _logger.LogDebug("Using static bearer token for external CRM request");
        }

        // Add Bearer token to Authorization header
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
