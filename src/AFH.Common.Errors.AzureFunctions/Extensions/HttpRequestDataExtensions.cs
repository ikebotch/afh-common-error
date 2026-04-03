using AFH.Common.Errors.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace AFH.Common.Errors.AzureFunctions.Extensions;

public static class HttpRequestDataExtensions
{
    public static ErrorContext ToErrorContext(this HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new ErrorContext(
            TraceId: request.FunctionContext.InvocationId,
            CorrelationId: request.GetCorrelationId(),
            Path: request.Url.AbsolutePath,
            Method: request.Method,
            Operation: request.FunctionContext.FunctionDefinition?.Name,
            Metadata: new Dictionary<string, string?>
            {
                ["host"] = request.Url.Host
            });
    }

    public static string? GetCorrelationId(this HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.TryGetHeaderValue("x-correlation-id")
            ?? request.TryGetHeaderValue("traceparent")
            ?? request.FunctionContext.GetCorrelationId();
    }

    public static string? TryGetHeaderValue(this HttpRequestData request, string headerName)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        if (!request.Headers.TryGetValues(headerName, out var values))
        {
            return null;
        }

        return values.FirstOrDefault();
    }
}
