using System.Text.Json;
using AFH.Common.Errors.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace AFH.Common.Errors.AzureFunctions.Responses;

public sealed class FunctionErrorResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task WriteAsync(
        HttpResponseData response,
        ErrorResponse errorResponse,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(errorResponse);

        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        response.Body.SetLength(0);
        await JsonSerializer.SerializeAsync(
            response.Body,
            errorResponse,
            errorResponse.GetType(),
            SerializerOptions,
            cancellationToken);
        response.Body.Position = 0;
    }
}
