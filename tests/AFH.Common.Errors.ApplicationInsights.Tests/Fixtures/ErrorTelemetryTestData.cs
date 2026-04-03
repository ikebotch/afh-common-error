using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.ApplicationInsights.Tests.Fixtures;

internal static class ErrorTelemetryTestData
{
    public static ErrorRecord CreateRecord(IReadOnlyDictionary<string, string?>? metadata = null)
    {
        return new ErrorRecord
        {
            Code = DependencyErrorCodes.Failure.Value,
            Category = ErrorCategory.Dependency,
            Severity = ErrorSeverity.Error,
            Message = "Dependency call failed.",
            ExceptionType = typeof(TimeoutException).FullName,
            StackTrace = "stack",
            OccurredUtc = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero),
            Context = new ErrorContext(
                TraceId: "trace-1",
                CorrelationId: "corr-1",
                Path: "/api/dependency",
                Method: "GET",
                Operation: "dependency.fetch",
                UserId: "user-1",
                Metadata: metadata ?? new Dictionary<string, string?> { ["tenantId"] = "tenant-1" }),
            Details =
            [
                new ErrorDetail(DependencyErrorCodes.Failure.Value, "Dependency call failed.")
            ]
        };
    }
}
