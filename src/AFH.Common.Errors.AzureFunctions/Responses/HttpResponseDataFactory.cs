using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace AFH.Common.Errors.AzureFunctions.Responses;

public sealed class HttpResponseDataFactory
{
    public HttpResponseData Create(HttpRequestData request, HttpStatusCode statusCode)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = request.CreateResponse();
        response.StatusCode = statusCode;
        return response;
    }
}
