namespace CRMBackEnd.Domain.Entities;

/// <summary>
/// Customer entity representing client data from external CRM service
/// </summary>
public class Customer
{
    public string ClientId { get; set; } = string.Empty;
    public string EditApproval { get; set; } = string.Empty;
    public string Dba { get; set; } = string.Empty;
    public string ClientLegalName { get; set; } = string.Empty;
    public string ComplianceHold { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string PaymentTermID { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
