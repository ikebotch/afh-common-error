namespace AFH.Common.Errors.Models;

public sealed class ValidationErrorResponse : ErrorResponse
{
    public IReadOnlyList<ValidationErrorDetail> ValidationErrors { get; init; } = [];
}
