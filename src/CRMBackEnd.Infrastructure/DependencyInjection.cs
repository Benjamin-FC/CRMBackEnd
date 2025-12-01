using CRMBackEnd.Application.Interfaces;
using CRMBackEnd.Application.Services;
using CRMBackEnd.Domain.Interfaces;
using CRMBackEnd.Infrastructure.Configuration;
using CRMBackEnd.Infrastructure.ExternalServices;
using CRMBackEnd.Infrastructure.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRMBackEnd.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind external CRM service settings
        var crmSettings = configuration.GetSection("ExternalCRMService").Get<ExternalCRMServiceSettings>()
            ?? throw new InvalidOperationException("ExternalCRMService configuration is missing");

        // Determine which environment settings to use
        var useDevelopment = configuration.GetValue<bool>("UseDevelopmentCRM");
        var activeSettings = useDevelopment ? crmSettings.DevelopmentCRM : crmSettings.Production;

        services.Configure<ExternalCRMServiceSettings>(
            configuration.GetSection("ExternalCRMService"));

        // Store configuration for later use
        services.AddSingleton(activeSettings);

        // Register HttpClient with BearerTokenHandler for external CRM service
        services.AddTransient<BearerTokenHandler>();

        services.AddHttpClient<ICRMServiceClient, CRMServiceClient>(client =>
        {
            client.BaseAddress = new Uri(activeSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(activeSettings.TimeoutSeconds);
        })
        .AddHttpMessageHandler<BearerTokenHandler>();

        // Register application services
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
