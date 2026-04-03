using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Models;

public sealed class ErrorNotificationRequest
{
    public required string Subject { get; init; }

    public required string Summary { get; init; }

    public required ErrorSeverity Severity { get; init; }

    public required ErrorRecord Record { get; init; }

    public IReadOnlyDictionary<string, string?> Metadata { get; init; } = new Dictionary<string, string?>();
}
