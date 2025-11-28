using CRMBackEnd.Domain.Entities;

namespace CRMBackEnd.Domain.Interfaces;

/// <summary>
/// Interface for external CRM service client
/// </summary>
public interface ICRMServiceClient
{
    /// <summary>
    /// Gets client data from external CRM service
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Customer entity</returns>
    Task<Customer> GetClientDataAsync(int id);
}
