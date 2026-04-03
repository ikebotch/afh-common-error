using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Email.Models;

public sealed class ErrorEmailTemplateModel
{
    public required string Subject { get; init; }

    public required string Summary { get; init; }

    public required ErrorSeverity Severity { get; init; }

    public required string ErrorCode { get; init; }

    public required ErrorCategory Category { get; init; }

    public DateTimeOffset OccurredUtc { get; init; }

    public string? TraceId { get; init; }

    public string? CorrelationId { get; init; }

    public string? ExceptionType { get; init; }

    public IReadOnlyList<ErrorDetail> Details { get; init; } = [];

    public IReadOnlyDictionary<string, string?> Metadata { get; init; } = new Dictionary<string, string?>();

    public IReadOnlyCollection<string> ToAddresses { get; init; } = [];

    public IReadOnlyCollection<string> CcAddresses { get; init; } = [];

    public IReadOnlyCollection<string> BccAddresses { get; init; } = [];

    public string? FromAddress { get; init; }

    public string? FromDisplayName { get; init; }
}
