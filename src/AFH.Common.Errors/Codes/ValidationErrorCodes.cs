namespace AFH.Common.Errors.Codes;

public static class ValidationErrorCodes
{
    public static readonly ErrorCode InvalidInput =
        new("validation.invalid_input", ErrorCategory.Validation, ErrorSeverity.Warning, "One or more validation errors occurred.");

    public static readonly ErrorCode Required =
        new("validation.required", ErrorCategory.Validation, ErrorSeverity.Warning, "A required value is missing.");

    public static readonly ErrorCode InvalidFormat =
        new("validation.invalid_format", ErrorCategory.Validation, ErrorSeverity.Warning, "The supplied value has an invalid format.");

    public static readonly ErrorCode OutOfRange =
        new("validation.out_of_range", ErrorCategory.Validation, ErrorSeverity.Warning, "The supplied value is outside the allowed range.");
}
