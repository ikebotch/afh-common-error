using AFH.Common.Errors.Mapping;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Abstractions;

public interface IErrorResponseBuilder
{
    ErrorResponse Build(Exception exception, ErrorContext? context = null);

    ErrorResponse Build(ExceptionMappingResult mappingResult);
}
