using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Abstractions;

public interface IErrorContextAccessor
{
    ErrorContext? Current { get; }
}
