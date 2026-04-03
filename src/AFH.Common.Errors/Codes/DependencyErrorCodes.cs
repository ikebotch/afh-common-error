namespace AFH.Common.Errors.Codes;

public static class DependencyErrorCodes
{
    public static readonly ErrorCode Failure =
        new("dependency.failure", ErrorCategory.Dependency, ErrorSeverity.Error, "A downstream dependency failed.");

    public static readonly ErrorCode Timeout =
        new("dependency.timeout", ErrorCategory.Dependency, ErrorSeverity.Error, "A downstream dependency timed out.");

    public static readonly ErrorCode Unavailable =
        new("dependency.unavailable", ErrorCategory.Dependency, ErrorSeverity.Error, "A downstream dependency is unavailable.");
}
