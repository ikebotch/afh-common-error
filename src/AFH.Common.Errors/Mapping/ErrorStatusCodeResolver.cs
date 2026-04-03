using AFH.Common.Errors.Codes;

namespace AFH.Common.Errors.Mapping;

public static class ErrorStatusCodeResolver
{
    public static int Resolve(ErrorCode errorCode)
    {
        if (errorCode == AuthErrorCodes.Forbidden || errorCode == CommonErrorCodes.Forbidden)
        {
            return 403;
        }

        if (errorCode == AuthErrorCodes.Unauthorized || errorCode == CommonErrorCodes.Unauthorized)
        {
            return 401;
        }

        if (errorCode == CommonErrorCodes.NotFound)
        {
            return 404;
        }

        if (errorCode == CommonErrorCodes.Conflict || errorCode == PersistenceErrorCodes.ConcurrencyConflict)
        {
            return 409;
        }

        if (errorCode == DependencyErrorCodes.Timeout)
        {
            return 504;
        }

        return errorCode.Category switch
        {
            ErrorCategory.Validation => 400,
            ErrorCategory.Authorization => 401,
            ErrorCategory.NotFound => 404,
            ErrorCategory.Conflict => 409,
            ErrorCategory.Dependency => 503,
            ErrorCategory.Persistence => 500,
            _ => 500
        };
    }
}
