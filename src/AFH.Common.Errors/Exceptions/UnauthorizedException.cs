using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public sealed class UnauthorizedException : AfhException
{
    public UnauthorizedException(string? message = null, Exception? innerException = null)
        : base(AuthErrorCodes.Unauthorized, message, innerException)
    {
    }
}
