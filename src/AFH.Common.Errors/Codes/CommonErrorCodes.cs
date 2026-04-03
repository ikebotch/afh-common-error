namespace AFH.Common.Errors.Codes;

public static class CommonErrorCodes
{
    public static readonly ErrorCode Unexpected =
        new("common.unexpected", ErrorCategory.Unknown, ErrorSeverity.Critical, "An unexpected error occurred.");

    public static readonly ErrorCode NotFound =
        new("common.not_found", ErrorCategory.NotFound, ErrorSeverity.Warning, "The requested resource was not found.");

    public static readonly ErrorCode Conflict =
        new("common.conflict", ErrorCategory.Conflict, ErrorSeverity.Warning, "The request could not be completed because of a conflict.");

    public static readonly ErrorCode Unauthorized =
        new("common.unauthorized", ErrorCategory.Authorization, ErrorSeverity.Warning, "Authentication is required.");

    public static readonly ErrorCode Forbidden =
        new("common.forbidden", ErrorCategory.Authorization, ErrorSeverity.Warning, "The current user is not allowed to perform this action.");
}
