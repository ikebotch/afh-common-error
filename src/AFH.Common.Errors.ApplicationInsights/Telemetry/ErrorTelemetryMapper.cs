using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.ApplicationInsights.Telemetry;

public sealed class ErrorTelemetryMapper
{
    public ErrorTelemetry Map(ErrorRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var properties = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [ErrorTelemetryConstants.PropertyKeys.ErrorCode] = record.Code,
            [ErrorTelemetryConstants.PropertyKeys.ErrorCategory] = record.Category.ToString(),
            [ErrorTelemetryConstants.PropertyKeys.ErrorSeverity] = record.Severity.ToString(),
            [ErrorTelemetryConstants.PropertyKeys.ErrorMessage] = record.Message,
            [ErrorTelemetryConstants.PropertyKeys.ErrorExceptionType] = record.ExceptionType,
            [ErrorTelemetryConstants.PropertyKeys.ErrorTraceId] = record.Context?.TraceId,
            [ErrorTelemetryConstants.PropertyKeys.ErrorCorrelationId] = record.Context?.CorrelationId,
            [ErrorTelemetryConstants.PropertyKeys.ErrorOperation] = record.Context?.Operation,
            [ErrorTelemetryConstants.PropertyKeys.ErrorPath] = record.Context?.Path,
            [ErrorTelemetryConstants.PropertyKeys.ErrorMethod] = record.Context?.Method,
            [ErrorTelemetryConstants.PropertyKeys.ErrorUserId] = record.Context?.UserId
        };

        if (record.Context?.Metadata is not null)
        {
            foreach (var pair in record.Context.Metadata)
            {
                if (properties.ContainsKey(pair.Key))
                {
                    continue;
                }

                properties[pair.Key] = pair.Value;
            }
        }

        var metrics = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            [ErrorTelemetryConstants.MetricKeys.DetailsCount] = record.Details.Count
        };

        return new ErrorTelemetry(
            ErrorTelemetryConstants.DefaultTelemetryName,
            record.OccurredUtc,
            properties,
            metrics);
    }
}
