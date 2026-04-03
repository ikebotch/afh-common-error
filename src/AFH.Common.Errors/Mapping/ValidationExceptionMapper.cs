using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Exceptions;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Mapping;

public sealed class ValidationExceptionMapper
{
    public ExceptionMappingResult Map(ValidationException exception, ErrorContext? context = null)
    {
        return new ExceptionMappingResult
        {
            Exception = exception,
            ErrorCode = exception.ErrorCode,
            Message = exception.Message,
            StatusCode = ErrorStatusCodeResolver.Resolve(ValidationErrorCodes.InvalidInput),
            Context = context,
            Details = exception.Errors
                .Select(error => new ErrorDetail(
                    error.Code ?? ValidationErrorCodes.InvalidInput.Value,
                    error.Message,
                    error.Field))
                .ToArray(),
            ValidationErrors = exception.Errors.ToArray()
        };
    }
}
