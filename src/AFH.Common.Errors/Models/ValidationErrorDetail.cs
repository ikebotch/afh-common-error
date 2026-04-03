namespace AFH.Common.Errors.Models;

public sealed record ValidationErrorDetail(
    string Field,
    string Message,
    string? Code = null,
    string? AttemptedValue = null);
