using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public class PersistenceFailureException : AfhException
{
    public PersistenceFailureException(ErrorCode? errorCode = null, string? message = null, Exception? innerException = null)
        : base(errorCode ?? PersistenceErrorCodes.Failure, message, innerException)
    {
    }
}
