using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public sealed class ForbiddenException : AfhException
{
    public ForbiddenException(string? message = null, Exception? innerException = null)
        : base(AuthErrorCodes.Forbidden, message, innerException)
    {
    }
}
