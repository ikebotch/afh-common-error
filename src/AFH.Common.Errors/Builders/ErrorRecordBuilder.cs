using AFH.Common.Errors.Mapping;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Builders;

public sealed class ErrorRecordBuilder
{
    public ErrorRecord Build(ExceptionMappingResult mappingResult)
    {
        ArgumentNullException.ThrowIfNull(mappingResult);

        return new ErrorRecord
        {
            Code = mappingResult.ErrorCode.Value,
            Category = mappingResult.ErrorCode.Category,
            Severity = mappingResult.ErrorCode.Severity,
            Message = mappingResult.Message,
            ExceptionType = mappingResult.Exception.GetType().FullName,
            StackTrace = mappingResult.Exception.StackTrace,
            Context = mappingResult.Context,
            Details = mappingResult.Details
        };
    }
}
