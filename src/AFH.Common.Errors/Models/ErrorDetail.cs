namespace AFH.Common.Errors.Models;

public sealed record ErrorDetail(
    string Code,
    string Message,
    string? Target = null,
    IReadOnlyDictionary<string, string?>? Metadata = null);
