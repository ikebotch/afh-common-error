using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public class DependencyFailureException : AfhException
{
    public DependencyFailureException(ErrorCode? errorCode = null, string? message = null, Exception? innerException = null)
        : base(errorCode ?? DependencyErrorCodes.Failure, message, innerException)
    {
    }
}
