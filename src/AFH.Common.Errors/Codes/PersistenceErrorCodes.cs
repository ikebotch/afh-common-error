namespace AFH.Common.Errors.Codes;

public static class PersistenceErrorCodes
{
    public static readonly ErrorCode Failure =
        new("persistence.failure", ErrorCategory.Persistence, ErrorSeverity.Error, "A persistence operation failed.");

    public static readonly ErrorCode ConcurrencyConflict =
        new("persistence.concurrency_conflict", ErrorCategory.Persistence, ErrorSeverity.Warning, "The record was changed by another operation.");

    public static readonly ErrorCode WriteFailed =
        new("persistence.write_failed", ErrorCategory.Persistence, ErrorSeverity.Error, "The data could not be written.");
}
