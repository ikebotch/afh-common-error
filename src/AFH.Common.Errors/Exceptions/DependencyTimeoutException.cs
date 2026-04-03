using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public sealed class DependencyTimeoutException : DependencyFailureException
{
    public DependencyTimeoutException(string? message = null, Exception? innerException = null)
        : base(DependencyErrorCodes.Timeout, message, innerException)
    {
    }
}
