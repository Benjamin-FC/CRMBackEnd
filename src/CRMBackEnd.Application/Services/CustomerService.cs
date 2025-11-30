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
        
        // Convert string ID to int for external API call
        if (!int.TryParse(id, out int customerId))
        {
            _logger.LogWarning("BACKEND: Invalid customer ID format: {CustomerId}", id);
            throw new ArgumentException($" BACKEND: Invalid customer ID: {id}. ID must be a valid integer.", nameof(id));
        }

        try
        {
            // Call external CRM service
            var customer = await _crmServiceClient.GetClientDataAsync(customerId);

            // Map to response DTO
            var response = _mapper.Map<CustomerInfoResponse>(customer);
            
            _logger.LogInformation("BACKEND: Successfully retrieved customer info for ID: {CustomerId}", id);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BACKEND:  Error retrieving customer info for ID: {CustomerId}", id);
            throw;
        }
    }
}
