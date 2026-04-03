namespace AFH.Common.Errors.Codes;

public sealed record ErrorCode(
    string Value,
    ErrorCategory Category,
    ErrorSeverity Severity,
    string DefaultMessage)
{
    public override string ToString()
    {
        return Value;
    }
}
