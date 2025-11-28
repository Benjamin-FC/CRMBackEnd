using System.Net.Http.Json;
using CRMBackEnd.Domain.Entities;
using CRMBackEnd.Domain.Interfaces;

namespace CRMBackEnd.Infrastructure.ExternalServices;

/// <summary>
/// Client for external CRM service
/// </summary>
public class CRMServiceClient : ICRMServiceClient
{
    private readonly HttpClient _httpClient;

    public CRMServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<Customer> GetClientDataAsync(int id)
    {
        try
        {
            // Call external CRM API endpoint
            var response = await _httpClient.GetAsync($"/api/v1/ClientData/{id}");

            // Ensure success status code
            response.EnsureSuccessStatusCode();

            // Deserialize response to Customer entity
            var customer = await response.Content.ReadFromJsonAsync<Customer>();

            if (customer == null)
            {
                throw new InvalidOperationException($"Failed to deserialize customer data for ID: {id}");
            }

            return customer;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error calling external CRM service for customer ID {id}: {ex.Message}", ex);
        }
    }
}
