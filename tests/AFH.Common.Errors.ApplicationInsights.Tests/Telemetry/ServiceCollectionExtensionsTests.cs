using AFH.Common.Errors.ApplicationInsights.DependencyInjection;
using AFH.Common.Errors.ApplicationInsights.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AFH.Common.Errors.ApplicationInsights.Tests.Telemetry;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAfhCommonErrorsApplicationInsights_RegistersTelemetryServices()
    {
        var provider = new ServiceCollection()
            .AddAfhCommonErrorsApplicationInsights()
            .BuildServiceProvider();

        provider.GetService<ErrorTelemetryMapper>().Should().NotBeNull();
        provider.GetService<ErrorTelemetryEnricher>().Should().NotBeNull();
        provider.GetService<ErrorTelemetryBuilder>().Should().NotBeNull();
    }
}
