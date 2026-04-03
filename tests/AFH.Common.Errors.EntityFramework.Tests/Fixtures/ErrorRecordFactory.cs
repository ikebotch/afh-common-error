using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.EntityFramework.Tests.Fixtures;

internal static class ErrorRecordFactory
{
    public static ErrorRecord Create()
    {
        return new ErrorRecord
        {
            Code = DependencyErrorCodes.Failure.Value,
            Category = ErrorCategory.Dependency,
            Severity = ErrorSeverity.Error,
            Message = "Dependency failed.",
            ExceptionType = typeof(TimeoutException).FullName,
            OccurredUtc = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero),
            Context = new ErrorContext(
                TraceId: "trace-1",
                CorrelationId: "corr-1",
                Path: "/errors",
                Method: "POST",
                Operation: "PersistError",
                UserId: "user-1",
                Metadata: new Dictionary<string, string?> { ["tenantId"] = "tenant-1" }),
            Details =
            [
                new ErrorDetail(DependencyErrorCodes.Failure.Value, "Dependency failed.")
            ]
        };
    }

    public static ErrorRecord CreateWithoutContextOrDetails()
    {
        return new ErrorRecord
        {
            Code = CommonErrorCodes.Unexpected.Value,
            Category = ErrorCategory.Unknown,
            Severity = ErrorSeverity.Critical,
            Message = "Unexpected."
        };
    }
}
