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

            _logger.LogInformation("BACKEND: External CRM API returned status: {StatusCode}", response.StatusCode);

            // Handle different status codes
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                var errorContent = await response.Content.ReadAsStringAsync();
                
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        _logger.LogWarning("BACKEND: Customer not found in external CRM. ID: {CustomerId}", id);
                        throw new KeyNotFoundException($"Customer with ID {id} not found in external CRM system");
                    
                    case System.Net.HttpStatusCode.Unauthorized:
                        _logger.LogError("BACKEND: Unauthorized access to external CRM for customer ID: {CustomerId}", id);
                        throw new UnauthorizedAccessException($"Authentication failed when accessing external CRM for customer ID {id}");
                    
                    case System.Net.HttpStatusCode.InternalServerError:
                        _logger.LogError("BACKEND: External CRM service error for customer ID: {CustomerId}. Response: {ErrorContent}", id, errorContent);
                        throw new InvalidOperationException($"External CRM service encountered an error processing customer ID {id}");
                    
                    default:
                        _logger.LogError("BACKEND: External CRM API error. Status: {StatusCode}, Customer ID: {CustomerId}, Response: {ErrorContent}", statusCode, id, errorContent);
                        throw new HttpRequestException($"External CRM API returned status code {statusCode} for customer ID {id}");
                }
            }

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
            throw new Exception($"BACKEND: Network error calling external CRM service for customer ID {id}: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is KeyNotFoundException or UnauthorizedAccessException or InvalidOperationException)
        {
            // Re-throw domain exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BACKEND: Unexpected error retrieving customer data for ID: {CustomerId}", id);
            throw;
        }
    }
}
