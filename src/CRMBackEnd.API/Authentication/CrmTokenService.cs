using System.Text.Json.Serialization;

namespace CRMBackEnd.API.Authentication;

/// <summary>
/// Service for obtaining OAuth tokens from the CRM authentication service
/// </summary>
public class CrmTokenService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CrmTokenService> _logger;
    private readonly IConfiguration _configuration;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public CrmTokenService(HttpClient httpClient, ILogger<CrmTokenService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Gets a valid bearer token, retrieving a new one if necessary
    /// </summary>
    public async Task<string> GetTokenAsync()
    {
        // Return cached token if still valid
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            _logger.LogDebug("Using cached CRM authentication token");
            return _cachedToken;
        }

        _logger.LogInformation("Requesting new CRM authentication token");
        
        var tokenUrl = _configuration["CrmAuthentication:TokenUrl"] 
            ?? throw new InvalidOperationException("CrmAuthentication:TokenUrl configuration is missing");
        var clientId = _configuration["CrmAuthentication:ClientId"] 
            ?? throw new InvalidOperationException("CrmAuthentication:ClientId configuration is missing");
        var clientSecret = _configuration["CrmAuthentication:ClientSecret"] 
            ?? throw new InvalidOperationException("CrmAuthentication:ClientSecret configuration is missing");
        var scope = _configuration["CrmAuthentication:Scope"] 
            ?? throw new InvalidOperationException("CrmAuthentication:Scope configuration is missing");
        var username = Environment.GetEnvironmentVariable("CRM_USERNAME") 
            ?? throw new InvalidOperationException("CRM_USERNAME environment variable is not set");
        var password = Environment.GetEnvironmentVariable("CRM_PASSWORD") 
            ?? throw new InvalidOperationException("CRM_PASSWORD environment variable is not set");

        var requestBody = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "scope", scope },
            { "grant_type", "password" },
            { "username", username },
            { "password", password }
        };

        try
        {
            var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(requestBody));
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse?.AccessToken == null)
            {
                throw new InvalidOperationException("Failed to retrieve access token from response");
            }

            _cachedToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            
            _logger.LogInformation("Successfully obtained CRM authentication token, expires in {ExpiresIn} seconds", tokenResponse.ExpiresIn);
            
            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain CRM authentication token from {TokenUrl}", tokenUrl);
            throw;
        }
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}
