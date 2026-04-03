using AFH.Common.Errors.ApplicationInsights.Telemetry;
using AFH.Common.Errors.ApplicationInsights.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.ApplicationInsights.Tests.Telemetry;

public sealed class ErrorTelemetryMapperTests
{
    [Fact]
    public void Map_BuildsTelemetryWithPropertiesAndMetrics()
    {
        var mapper = new ErrorTelemetryMapper();
        var record = ErrorTelemetryTestData.CreateRecord();

        var telemetry = mapper.Map(record);

        telemetry.Name.Should().Be(ErrorTelemetryConstants.DefaultTelemetryName);
        telemetry.Timestamp.Should().Be(record.OccurredUtc);
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorCode].Should().Be(record.Code);
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorCategory].Should().Be(record.Category.ToString());
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorSeverity].Should().Be(record.Severity.ToString());
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorTraceId].Should().Be("trace-1");
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorCorrelationId].Should().Be("corr-1");
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorOperation].Should().Be("dependency.fetch");
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorPath].Should().Be("/api/dependency");
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorMethod].Should().Be("GET");
        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorUserId].Should().Be("user-1");
        telemetry.Properties.Should().ContainKey("tenantId").WhoseValue.Should().Be("tenant-1");
        telemetry.Metrics[ErrorTelemetryConstants.MetricKeys.DetailsCount].Should().Be(1);
    }

    [Fact]
    public void Map_DoesNotOverridePropertyKeysFromMetadata()
    {
        var mapper = new ErrorTelemetryMapper();
        var metadata = new Dictionary<string, string?>
        {
            [ErrorTelemetryConstants.PropertyKeys.ErrorCode] = "override",
            ["custom"] = "value"
        };
        var record = ErrorTelemetryTestData.CreateRecord(metadata);

        var telemetry = mapper.Map(record);

        telemetry.Properties[ErrorTelemetryConstants.PropertyKeys.ErrorCode].Should().Be(record.Code);
        telemetry.Properties.Should().ContainKey("custom").WhoseValue.Should().Be("value");
    }
}
