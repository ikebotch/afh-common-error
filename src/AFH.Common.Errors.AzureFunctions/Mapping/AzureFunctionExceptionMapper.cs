using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.AzureFunctions.Extensions;
using AFH.Common.Errors.Mapping;
using AFH.Common.Errors.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AFH.Common.Errors.AzureFunctions.Mapping;

public sealed class AzureFunctionExceptionMapper
{
    private readonly IExceptionMapper _exceptionMapper;

    public AzureFunctionExceptionMapper(IExceptionMapper exceptionMapper)
    {
        _exceptionMapper = exceptionMapper;
    }

    public ExceptionMappingResult Map(Exception exception, FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return _exceptionMapper.Map(exception, context.ToErrorContext());
    }

    public ExceptionMappingResult Map(Exception exception, HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _exceptionMapper.Map(exception, request.ToErrorContext());
    }

    public ExceptionMappingResult Map(Exception exception, FunctionContext context, HttpRequestData request)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);

        var requestContext = request.ToErrorContext();
        var contextData = context.ToErrorContext();

        var mergedContext = new ErrorContext(
            TraceId: requestContext.TraceId ?? contextData.TraceId,
            CorrelationId: requestContext.CorrelationId ?? contextData.CorrelationId,
            Path: requestContext.Path,
            Method: requestContext.Method,
            Operation: contextData.Operation,
            UserId: contextData.UserId,
            Metadata: requestContext.Metadata ?? contextData.Metadata);

        return _exceptionMapper.Map(exception, mergedContext);
    }
}
