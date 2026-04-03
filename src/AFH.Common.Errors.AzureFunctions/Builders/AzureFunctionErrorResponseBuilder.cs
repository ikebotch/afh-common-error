using AFH.Common.Errors.Builders;
using AFH.Common.Errors.AzureFunctions.Mapping;
using AFH.Common.Errors.AzureFunctions.Responses;
using AFH.Common.Errors.Mapping;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AFH.Common.Errors.AzureFunctions.Builders;

public sealed class AzureFunctionErrorResponseBuilder
{
    private readonly AzureFunctionExceptionMapper _exceptionMapper;
    private readonly ErrorResponseBuilder _errorResponseBuilder;
    private readonly HttpResponseDataFactory _responseFactory;
    private readonly FunctionErrorResponseWriter _responseWriter;

    public AzureFunctionErrorResponseBuilder(
        AzureFunctionExceptionMapper exceptionMapper,
        ErrorResponseBuilder errorResponseBuilder,
        HttpResponseDataFactory responseFactory,
        FunctionErrorResponseWriter responseWriter)
    {
        _exceptionMapper = exceptionMapper;
        _errorResponseBuilder = errorResponseBuilder;
        _responseFactory = responseFactory;
        _responseWriter = responseWriter;
    }

    public async Task<HttpResponseData> BuildAsync(
        HttpRequestData request,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var mappingResult = _exceptionMapper.Map(exception, request);
        return await BuildAsync(request, mappingResult, cancellationToken);
    }

    public async Task<HttpResponseData> BuildAsync(
        FunctionContext context,
        HttpRequestData request,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var mappingResult = _exceptionMapper.Map(exception, context, request);
        return await BuildAsync(request, mappingResult, cancellationToken);
    }

    public async Task<HttpResponseData> BuildAsync(
        HttpRequestData request,
        ExceptionMappingResult mappingResult,
        CancellationToken cancellationToken = default)
    {
        var response = _responseFactory.Create(request, HttpStatusCodeResolver.Resolve(mappingResult));
        var errorResponse = _errorResponseBuilder.Build(mappingResult);
        await _responseWriter.WriteAsync(response, errorResponse, cancellationToken);
        return response;
    }
}
