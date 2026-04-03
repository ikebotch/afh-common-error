using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public class ConflictException : AfhException
{
    public ConflictException(ErrorCode? errorCode = null, string? message = null, Exception? innerException = null)
        : base(errorCode ?? CommonErrorCodes.Conflict, message, innerException)
    {
    }
}
