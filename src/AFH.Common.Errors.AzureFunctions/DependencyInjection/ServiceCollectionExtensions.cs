using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.AzureFunctions.Builders;
using AFH.Common.Errors.AzureFunctions.Mapping;
using AFH.Common.Errors.AzureFunctions.Responses;
using AFH.Common.Errors.Builders;
using AFH.Common.Errors.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace AFH.Common.Errors.AzureFunctions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAfhCommonErrorsAzureFunctions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IExceptionMapper, DefaultExceptionMapper>();
        services.AddSingleton<ErrorResponseBuilder>();
        services.AddSingleton<AzureFunctionExceptionMapper>();
        services.AddSingleton<HttpResponseDataFactory>();
        services.AddSingleton<FunctionErrorResponseWriter>();
        services.AddSingleton<AzureFunctionErrorResponseBuilder>();

        return services;
    }
}
