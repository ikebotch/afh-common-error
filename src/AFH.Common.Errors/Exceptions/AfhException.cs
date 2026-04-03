using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Exceptions;

public class AfhException : Exception
{
    public AfhException(
        ErrorCode errorCode,
        string? message = null,
        Exception? innerException = null,
        IReadOnlyDictionary<string, string?>? metadata = null)
        : base(message ?? errorCode.DefaultMessage, innerException)
    {
        ErrorCode = errorCode;
        Metadata = metadata ?? new Dictionary<string, string?>();
    }

    public ErrorCode ErrorCode { get; }

    public IReadOnlyDictionary<string, string?> Metadata { get; }
}
