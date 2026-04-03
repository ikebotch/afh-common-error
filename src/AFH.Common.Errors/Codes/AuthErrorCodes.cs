namespace AFH.Common.Errors.Codes;

public static class AuthErrorCodes
{
    public static readonly ErrorCode Unauthorized = CommonErrorCodes.Unauthorized;

    public static readonly ErrorCode Forbidden = CommonErrorCodes.Forbidden;

    public static readonly ErrorCode InvalidCredentials =
        new("auth.invalid_credentials", ErrorCategory.Authorization, ErrorSeverity.Warning, "The supplied credentials are invalid.");

    public static readonly ErrorCode SessionExpired =
        new("auth.session_expired", ErrorCategory.Authorization, ErrorSeverity.Warning, "The current session has expired.");
}
