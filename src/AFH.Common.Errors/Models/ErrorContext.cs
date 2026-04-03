namespace AFH.Common.Errors.Models;

public sealed record ErrorContext(
    string? TraceId = null,
    string? CorrelationId = null,
    string? Path = null,
    string? Method = null,
    string? Operation = null,
    string? UserId = null,
    IReadOnlyDictionary<string, string?>? Metadata = null);
