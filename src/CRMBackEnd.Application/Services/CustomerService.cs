using AutoMapper;
using CRMBackEnd.Application.DTOs;
using CRMBackEnd.Application.Interfaces;
using CRMBackEnd.Domain.Interfaces;

namespace CRMBackEnd.Application.Services;

/// <summary>
/// Service implementation for customer operations
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICRMServiceClient _crmServiceClient;
    private readonly IMapper _mapper;

    public CustomerService(ICRMServiceClient crmServiceClient, IMapper mapper)
    {
        _crmServiceClient = crmServiceClient;
        _mapper = mapper;
    }

    public async Task<CustomerInfoResponse> GetCustomerInfoAsync(string id)
    {
        // Convert string ID to int for external API call
        if (!int.TryParse(id, out int customerId))
        {
            throw new ArgumentException($"Invalid customer ID: {id}. ID must be a valid integer.", nameof(id));
        }

        // Call external CRM service
        var customer = await _crmServiceClient.GetClientDataAsync(customerId);

        // Map to response DTO
        return _mapper.Map<CustomerInfoResponse>(customer);
    }
}
