using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Exceptions;

public sealed class ValidationException : AfhException
{
    public ValidationException(
        IReadOnlyCollection<ValidationErrorDetail> errors,
        string? message = null,
        Exception? innerException = null)
        : base(ValidationErrorCodes.InvalidInput, message, innerException)
    {
        Errors = errors;
    }

    public IReadOnlyCollection<ValidationErrorDetail> Errors { get; }
}
