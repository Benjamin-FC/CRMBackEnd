using AutoMapper;
using CRMBackEnd.Application.DTOs;
using CRMBackEnd.Application.Interfaces;
using CRMBackEnd.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRMBackEnd.Application.Services;

/// <summary>
/// Service implementation for customer operations
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICRMServiceClient _crmServiceClient;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICRMServiceClient crmServiceClient, IMapper mapper, ILogger<CustomerService> logger)
    {
        _crmServiceClient = crmServiceClient;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerInfoResponse> GetCustomerInfoAsync(string id)
    {
        _logger.LogInformation("BACKEND: Getting customer info for ID: {CustomerId}", id);
        
        // Validate input
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("BACKEND: Empty or null customer ID provided");
            throw new ArgumentException("BACKEND: Customer ID cannot be null or empty.", nameof(id));
        }
        
        // Convert string ID to int for external API call
        if (!int.TryParse(id, out int customerId))
        {
            _logger.LogWarning("BACKEND: Invalid customer ID format: {CustomerId}", id);
            throw new ArgumentException($"BACKEND: Invalid customer ID: {id}. ID must be a valid integer.", nameof(id));
        }

        // Validate ID is positive
        if (customerId <= 0)
        {
            _logger.LogWarning("BACKEND: Invalid customer ID value: {CustomerId}", customerId);
            throw new ArgumentException($"BACKEND: Invalid customer ID: {customerId}. ID must be a positive integer.", nameof(id));
        }

        try
        {
            // Call external CRM service
            var customer = await _crmServiceClient.GetClientDataAsync(customerId);

            // Validate response
            if (customer == null)
            {
                _logger.LogWarning("BACKEND: No customer data returned for ID: {CustomerId}", customerId);
                throw new InvalidOperationException($"BACKEND: No customer data found for ID: {customerId}");
            }

            // Map to response DTO
            var response = _mapper.Map<CustomerInfoResponse>(customer);
            
            _logger.LogInformation("BACKEND: Successfully retrieved customer info for ID: {CustomerId}", id);
            return response;
        }
        catch (ArgumentException)
        {
            // Re-throw validation exceptions
            throw;
        }
        catch (InvalidOperationException)
        {
            // Re-throw operational exceptions
            throw;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "BACKEND: HTTP error while retrieving customer info for ID: {CustomerId}", id);
            throw new InvalidOperationException($"BACKEND: Failed to communicate with external CRM service for customer ID: {customerId}", httpEx);
        }
        catch (TimeoutException timeoutEx)
        {
            _logger.LogError(timeoutEx, "BACKEND: Timeout while retrieving customer info for ID: {CustomerId}", id);
            throw new InvalidOperationException($"BACKEND: Request timed out while retrieving customer ID: {customerId}", timeoutEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BACKEND: Unexpected error retrieving customer info for ID: {CustomerId}", id);
            throw new InvalidOperationException($"BACKEND: An unexpected error occurred while retrieving customer ID: {customerId}. See inner exception for details.", ex);
        }
    }
}
