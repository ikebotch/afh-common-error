using AFH.Common.Errors.Mapping;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Builders;

public sealed class ValidationErrorBuilder
{
    public ValidationErrorResponse Build(ExceptionMappingResult mappingResult)
    {
        ArgumentNullException.ThrowIfNull(mappingResult);

        return new ValidationErrorResponse
        {
            StatusCode = mappingResult.StatusCode,
            TraceId = mappingResult.Context?.TraceId,
            CorrelationId = mappingResult.Context?.CorrelationId,
            Error = new ErrorDetail(mappingResult.ErrorCode.Value, mappingResult.Message),
            Details = mappingResult.Details,
            ValidationErrors = mappingResult.ValidationErrors
        };
    }
}
