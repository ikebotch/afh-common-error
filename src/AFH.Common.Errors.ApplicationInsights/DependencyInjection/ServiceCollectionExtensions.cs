using AFH.Common.Errors.ApplicationInsights.Telemetry;
using Microsoft.Extensions.DependencyInjection;

namespace AFH.Common.Errors.ApplicationInsights.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAfhCommonErrorsApplicationInsights(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ErrorTelemetryMapper>();
        services.AddSingleton<ErrorTelemetryEnricher>();
        services.AddSingleton<ErrorTelemetryBuilder>();

        return services;
    }
}
