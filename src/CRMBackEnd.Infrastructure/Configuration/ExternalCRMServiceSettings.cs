namespace CRMBackEnd.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for external CRM service
/// </summary>
public class ExternalCRMServiceSettings
{
    public CRMEnvironmentSettings Development { get; set; } = new();
    public CRMEnvironmentSettings Production { get; set; } = new();
}

/// <summary>
/// CRM service settings for a specific environment
/// </summary>
public class CRMEnvironmentSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string BearerToken { get; set; } = "123";
    public int TimeoutSeconds { get; set; } = 30;
}
