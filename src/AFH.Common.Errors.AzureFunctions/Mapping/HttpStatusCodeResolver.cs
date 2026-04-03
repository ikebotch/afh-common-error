using System.Net;
using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Mapping;

namespace AFH.Common.Errors.AzureFunctions.Mapping;

public static class HttpStatusCodeResolver
{
    public static HttpStatusCode Resolve(ErrorCode errorCode)
    {
        ArgumentNullException.ThrowIfNull(errorCode);
        return (HttpStatusCode)ErrorStatusCodeResolver.Resolve(errorCode);
    }

    public static HttpStatusCode Resolve(ExceptionMappingResult mappingResult)
    {
        ArgumentNullException.ThrowIfNull(mappingResult);
        return (HttpStatusCode)mappingResult.StatusCode;
    }
}
