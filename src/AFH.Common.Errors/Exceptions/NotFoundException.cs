using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public sealed class NotFoundException : AfhException
{
    public NotFoundException(string? message = null, Exception? innerException = null)
        : base(CommonErrorCodes.NotFound, message, innerException)
    {
    }
}
