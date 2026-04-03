using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Email.Options;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Email.Tests.Fixtures;

internal static class ErrorEmailTestData
{
    public static ErrorEmailOptions CreateOptions(bool includeDetails = true)
    {
        return new ErrorEmailOptions
        {
            FromAddress = "errors@afh.local",
            FromDisplayName = "AFH Errors",
            ToAddresses = ["ops@afh.local"],
            SubjectPrefix = "[Errors]",
            IncludeDetails = includeDetails
        };
    }

    public static ErrorNotificationRequest CreateRequest(IReadOnlyDictionary<string, string?>? metadata = null)
    {
        return new ErrorNotificationRequest
        {
            Subject = "Error: dependency.failure",
            Summary = "Dependency call failed.",
            Severity = ErrorSeverity.Error,
            Record = new ErrorRecord
            {
                Code = DependencyErrorCodes.Failure.Value,
                Category = ErrorCategory.Dependency,
                Severity = ErrorSeverity.Error,
                Message = "Dependency call failed.",
                ExceptionType = typeof(TimeoutException).FullName,
                OccurredUtc = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero),
                Context = new ErrorContext(
                    TraceId: "trace-1",
                    CorrelationId: "corr-1",
                    Metadata: new Dictionary<string, string?> { ["traceId"] = "trace-1" }),
                Details =
                [
                    new ErrorDetail(DependencyErrorCodes.Failure.Value, "Dependency call failed.")
                ]
            },
            Metadata = metadata ?? new Dictionary<string, string?> { ["traceId"] = "trace-1" }
        };
    }
}
