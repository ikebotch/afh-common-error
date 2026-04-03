using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Mapping;

public sealed class ExceptionMappingResult
{
    public required Exception Exception { get; init; }

    public required ErrorCode ErrorCode { get; init; }

    public required string Message { get; init; }

    public required int StatusCode { get; init; }

    public ErrorContext? Context { get; init; }

    public IReadOnlyList<ErrorDetail> Details { get; init; } = [];

    public IReadOnlyList<ValidationErrorDetail> ValidationErrors { get; init; } = [];
}
