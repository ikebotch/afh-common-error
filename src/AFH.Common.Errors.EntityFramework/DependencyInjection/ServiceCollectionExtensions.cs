using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.EntityFramework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AFH.Common.Errors.EntityFramework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAfhCommonErrorsEntityFramework<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IErrorPersistenceWriter, EntityFrameworkErrorPersistenceWriter<TContext>>();
        return services;
    }
}
