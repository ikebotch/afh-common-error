using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.Email.Builders;
using AFH.Common.Errors.Email.Models;
using AFH.Common.Errors.Email.Notifications;
using AFH.Common.Errors.Email.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AFH.Common.Errors.Email.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAfhCommonErrorsEmail(
        this IServiceCollection services,
        ErrorEmailOptions options,
        Func<IServiceProvider, Func<ErrorEmailTemplateModel, string, CancellationToken, Task>> senderFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(senderFactory);

        services.AddSingleton(options);
        services.AddSingleton<ErrorEmailMessageBuilder>();
        services.AddSingleton<IErrorNotifier>(serviceProvider =>
            new EmailErrorNotifier(
                serviceProvider.GetRequiredService<ErrorEmailOptions>(),
                serviceProvider.GetRequiredService<ErrorEmailMessageBuilder>(),
                senderFactory(serviceProvider)));

        return services;
    }
}
