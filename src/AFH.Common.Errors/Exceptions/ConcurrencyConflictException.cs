using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public sealed class ConcurrencyConflictException : ConflictException
{
    public ConcurrencyConflictException(string? message = null, Exception? innerException = null)
        : base(PersistenceErrorCodes.ConcurrencyConflict, message, innerException)
    {
    }
}
