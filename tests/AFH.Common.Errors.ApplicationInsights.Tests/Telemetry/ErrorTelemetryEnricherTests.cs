using AFH.Common.Errors.ApplicationInsights.Telemetry;
using AFH.Common.Errors.ApplicationInsights.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.ApplicationInsights.Tests.Telemetry;

public sealed class ErrorTelemetryEnricherTests
{
    [Fact]
    public void Enrich_FillsMissingDefaults()
    {
        var record = ErrorTelemetryTestData.CreateRecord();
        var properties = new Dictionary<string, string?>();
        var metrics = new Dictionary<string, double>();

        new ErrorTelemetryEnricher().Enrich(record, properties, metrics);

        properties[ErrorTelemetryConstants.PropertyKeys.ErrorCode].Should().Be(record.Code);
        properties[ErrorTelemetryConstants.PropertyKeys.ErrorSeverity].Should().Be(record.Severity.ToString());
        metrics[ErrorTelemetryConstants.MetricKeys.DetailsCount].Should().Be(1);
    }

    [Fact]
    public void Enrich_DoesNotOverrideExistingValues()
    {
        var record = ErrorTelemetryTestData.CreateRecord();
        var properties = new Dictionary<string, string?>
        {
            [ErrorTelemetryConstants.PropertyKeys.ErrorCode] = "custom",
            [ErrorTelemetryConstants.PropertyKeys.ErrorSeverity] = "Warning"
        };
        var metrics = new Dictionary<string, double>
        {
            [ErrorTelemetryConstants.MetricKeys.DetailsCount] = 42
        };

        new ErrorTelemetryEnricher().Enrich(record, properties, metrics);

        properties[ErrorTelemetryConstants.PropertyKeys.ErrorCode].Should().Be("custom");
        properties[ErrorTelemetryConstants.PropertyKeys.ErrorSeverity].Should().Be("Warning");
        metrics[ErrorTelemetryConstants.MetricKeys.DetailsCount].Should().Be(42);
    }
}
