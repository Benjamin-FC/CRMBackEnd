namespace CRMBackEnd.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for external CRM service
/// </summary>
public class ExternalCRMServiceSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string BearerToken { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
