using CRMBackEnd.Application.DTOs;

namespace CRMBackEnd.Application.Interfaces;

/// <summary>
/// Service interface for customer operations
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Gets customer information by ID
    /// </summary>
    /// <param name="id">Customer ID (string)</param>
    /// <returns>Customer information response</returns>
    Task<CustomerInfoResponse> GetCustomerInfoAsync(string id);
}
