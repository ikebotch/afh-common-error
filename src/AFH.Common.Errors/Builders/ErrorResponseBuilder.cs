using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.Exceptions;
using AFH.Common.Errors.Mapping;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Builders;

public sealed class ErrorResponseBuilder : IErrorResponseBuilder
{
    private readonly IExceptionMapper _exceptionMapper;
    private readonly ValidationErrorBuilder _validationErrorBuilder = new();

    public ErrorResponseBuilder(IExceptionMapper exceptionMapper)
    {
        _exceptionMapper = exceptionMapper;
    }

    public ErrorResponse Build(Exception exception, ErrorContext? context = null)
    {
        return Build(_exceptionMapper.Map(exception, context));
    }

    public ErrorResponse Build(ExceptionMappingResult mappingResult)
    {
        ArgumentNullException.ThrowIfNull(mappingResult);

        if (mappingResult.Exception is ValidationException)
        {
            return _validationErrorBuilder.Build(mappingResult);
        }

        return new ErrorResponse
        {
            StatusCode = mappingResult.StatusCode,
            TraceId = mappingResult.Context?.TraceId,
            CorrelationId = mappingResult.Context?.CorrelationId,
            Error = new ErrorDetail(mappingResult.ErrorCode.Value, mappingResult.Message),
            Details = mappingResult.Details
        };
    }
}
