using System.Text.Json.Serialization;

namespace CRMBackEnd.Domain.Entities;

/// <summary>
/// Customer entity representing client data from external CRM service
/// </summary>
public class Customer
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;
    
    [JsonPropertyName("editApproval")]
    public string EditApproval { get; set; } = string.Empty;
    
    [JsonPropertyName("dba")]
    public string Dba { get; set; } = string.Empty;
    
    [JsonPropertyName("clientLegalName")]
    public string ClientLegalName { get; set; } = string.Empty;
    
    [JsonPropertyName("complianceHold")]
    public string ComplianceHold { get; set; } = string.Empty;
    
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;
    
    [JsonPropertyName("paymentTermID")]
    public string PaymentTermID { get; set; } = string.Empty;
    
    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
