using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.ApplicationInsights.Telemetry;

public sealed class ErrorTelemetryEnricher
{
    public void Enrich(ErrorRecord record, IDictionary<string, string?> properties, IDictionary<string, double> metrics)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(metrics);

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorCode))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorCode] = record.Code;
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorCategory))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorCategory] = record.Category.ToString();
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorSeverity))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorSeverity] = record.Severity.ToString();
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorMessage))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorMessage] = record.Message;
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorExceptionType))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorExceptionType] = record.ExceptionType;
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorTraceId))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorTraceId] = record.Context?.TraceId;
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorCorrelationId))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorCorrelationId] = record.Context?.CorrelationId;
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorOperation))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorOperation] = record.Context?.Operation;
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorPath))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorPath] = record.Context?.Path;
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorMethod))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorMethod] = record.Context?.Method;
        }

        if (!properties.ContainsKey(ErrorTelemetryConstants.PropertyKeys.ErrorUserId))
        {
            properties[ErrorTelemetryConstants.PropertyKeys.ErrorUserId] = record.Context?.UserId;
        }

        if (!metrics.ContainsKey(ErrorTelemetryConstants.MetricKeys.DetailsCount))
        {
            metrics[ErrorTelemetryConstants.MetricKeys.DetailsCount] = record.Details.Count;
        }
    }
}
