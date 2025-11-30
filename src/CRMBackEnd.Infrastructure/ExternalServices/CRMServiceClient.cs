using System.Net.Http.Json;
using CRMBackEnd.Domain.Entities;
using CRMBackEnd.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRMBackEnd.Infrastructure.ExternalServices;

/// <summary>
/// Client for external CRM service
/// </summary>
public class CRMServiceClient : ICRMServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CRMServiceClient> _logger;

    public CRMServiceClient(HttpClient httpClient, ILogger<CRMServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Set Bearer token if not already present
        if (_httpClient.DefaultRequestHeaders.Authorization == null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "123");
        }
    }

    public async Task<Customer> GetClientDataAsync(int id)
    {
        _logger.LogInformation("BACKEND: Fetching customer data for ID: {CustomerId}", id);
        
        try
        {
            // Call external CRM API endpoint (no leading slash to preserve base URL path)
            var url = $"api/v1/ClientData/{id}";
            _logger.LogInformation("BACKEND: Calling external CRM API: {Url}", url);
            

            // Log the Bearer token being used
            var authHeader = _httpClient.DefaultRequestHeaders.Authorization;
            _logger.LogInformation ("BACKEND: Using Bearer token: {Token}", authHeader?.Parameter ?? "None");
            
            var response = await _httpClient.GetAsync(url);

            // Ensure success status code
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("BACKEND: External CRM API returned status: {StatusCode}", response.StatusCode);

            // Deserialize response to Customer entity
            var customer = await response.Content.ReadFromJsonAsync<Customer>();

            if (customer == null)
            {
                _logger.LogError("BACKEND: Failed to deserialize customer data for ID: {CustomerId}", id);
                throw new InvalidOperationException($"BACKEND: Failed to deserialize customer data for ID: {id}");
            }

            _logger.LogInformation("BACKEND: Successfully retrieved customer data for ID: {CustomerId}", id);
            return customer;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "BACKEND: HTTP error calling external CRM service for customer ID: {CustomerId}", id);
            throw new Exception($"BACKEND: Error calling external CRM service for customer ID {id}: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BACKEND: Unexpected error retrieving customer data for ID: {CustomerId}", id);
            throw;
        }
    }
}
