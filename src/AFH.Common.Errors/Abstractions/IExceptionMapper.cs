using AFH.Common.Errors.Mapping;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Abstractions;

public interface IExceptionMapper
{
    ExceptionMappingResult Map(Exception exception, ErrorContext? context = null);
}
