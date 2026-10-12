using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace Diagnostics;

public static class DiagnosticServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, 
        string serviceName,
        IConfiguration configuration
        )
    {
        var otelBuilder = services.AddOpenTelemetry();
        var appInsightsConnectionString =
            configuration["ApplicationInsights:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            otelBuilder.UseAzureMonitor(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });
        }

        return services;
    }
}
