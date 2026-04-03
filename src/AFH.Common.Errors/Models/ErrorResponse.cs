namespace AFH.Common.Errors.Models;

public class ErrorResponse
{
    public required ErrorDetail Error { get; init; }

    public int StatusCode { get; init; }

    public string? TraceId { get; init; }

    public string? CorrelationId { get; init; }

    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<ErrorDetail> Details { get; init; } = [];
}
