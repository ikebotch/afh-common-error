using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Exceptions;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Mapping;

public sealed class DefaultExceptionMapper : IExceptionMapper
{
    private readonly ValidationExceptionMapper _validationExceptionMapper = new();

    public ExceptionMappingResult Map(Exception exception, ErrorContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is ValidationException validationException)
        {
            return _validationExceptionMapper.Map(validationException, context);
        }

        var errorCode = ResolveErrorCode(exception);

        return new ExceptionMappingResult
        {
            Exception = exception,
            ErrorCode = errorCode,
            Message = exception.Message,
            StatusCode = ErrorStatusCodeResolver.Resolve(errorCode),
            Context = context,
            Details = []
        };
    }

    private static ErrorCode ResolveErrorCode(Exception exception)
    {
        return exception switch
        {
            AfhException afhException => afhException.ErrorCode,
            TimeoutException => DependencyErrorCodes.Timeout,
            KeyNotFoundException => CommonErrorCodes.NotFound,
            _ => CommonErrorCodes.Unexpected
        };
    }
}
