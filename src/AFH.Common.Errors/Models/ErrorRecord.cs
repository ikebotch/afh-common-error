using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Models;

public sealed class ErrorRecord
{
    public required string Code { get; init; }

    public required ErrorCategory Category { get; init; }

    public required ErrorSeverity Severity { get; init; }

    public required string Message { get; init; }

    public string? ExceptionType { get; init; }

    public string? StackTrace { get; init; }

    public ErrorContext? Context { get; init; }

    public IReadOnlyList<ErrorDetail> Details { get; init; } = [];

    public DateTimeOffset OccurredUtc { get; init; } = DateTimeOffset.UtcNow;
}
