using AFH.Common.Errors.ApplicationInsights.Telemetry;
using AFH.Common.Errors.ApplicationInsights.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.ApplicationInsights.Tests.Telemetry;

public sealed class ErrorTelemetryBuilderTests
{
    [Fact]
    public void Build_AllowsCustomEnrichmentThenFillsDefaults()
    {
        var builder = new ErrorTelemetryBuilder(new ErrorTelemetryMapper(), new ErrorTelemetryEnricher());
        var record = ErrorTelemetryTestData.CreateRecord();

        var telemetry = builder.Build(
            record,
            (properties, metrics) =>
            {
                properties[ErrorTelemetryConstants.PropertyKeys.ErrorSeverity] = "Warning";
                properties["custom"] = "value";
                metrics[ErrorTelemetryConstants.MetricKeys.DetailsCount] = 2;
                metrics["custom.metric"] = 99;
            });

        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorSeverity].Should().Be("Warning");
        telemetry.Properties["custom"].Should().Be("value");
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorCode].Should().Be(record.Code);
        telemetry.Metrics[ErrorTelemetryConstants.MetricKeys.DetailsCount].Should().Be(2);
        telemetry.Metrics["custom.metric"].Should().Be(99);
    }
}
