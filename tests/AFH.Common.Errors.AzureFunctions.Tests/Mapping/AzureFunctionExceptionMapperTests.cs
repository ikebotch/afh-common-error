using AFH.Common.Errors.AzureFunctions.Extensions;
using AFH.Common.Errors.AzureFunctions.Mapping;
using AFH.Common.Errors.AzureFunctions.Tests.Fixtures;
using AFH.Common.Errors.Exceptions;
using AFH.Common.Errors.Mapping;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.AzureFunctions.Tests.Mapping;

public sealed class AzureFunctionExceptionMapperTests
{
    [Fact]
    public void Map_WithRequest_UsesRequestContext()
    {
        var request = TestHttpRequestData.Create();
        request.Headers.Add("x-correlation-id", "corr-123");
        var mapper = new AzureFunctionExceptionMapper(new DefaultExceptionMapper());

        var result = mapper.Map(new NotFoundException("Missing."), request);

        result.StatusCode.Should().Be(404);
        result.Context?.CorrelationId.Should().Be("corr-123");
        result.Context?.Path.Should().Be("/clients/42");
        result.Context?.Method.Should().Be("GET");
        result.Context?.Metadata?["host"].Should().Be("localhost");
    }

    [Fact]
    public void Map_WithContextAndRequest_MergesFunctionAndRequestData()
    {
        var mapper = new AzureFunctionExceptionMapper(new DefaultExceptionMapper());
        var context = new TestFunctionContext();
        context.Items["CorrelationId"] = "ctx-correlation";
        var request = new TestHttpRequestData(context);
        request.Headers.Add("x-correlation-id", "req-correlation");

        var result = mapper.Map(new DependencyTimeoutException("Timed out."), context, request);

        result.StatusCode.Should().Be(504);
        result.Context?.CorrelationId.Should().Be("req-correlation");
        result.Context?.Operation.Should().Be("ErrorsFunction");
        result.Context?.Path.Should().Be("/clients/42");
    }

    [Fact]
    public void ToErrorContext_WithFunctionContext_ProjectsExpectedValues()
    {
        var context = new TestFunctionContext();
        context.Items["CorrelationId"] = "ctx-correlation";

        var errorContext = context.ToErrorContext();

        errorContext.TraceId.Should().Be("inv-123");
        errorContext.CorrelationId.Should().Be("ctx-correlation");
        errorContext.Operation.Should().Be("ErrorsFunction");
        errorContext.Metadata?["functionId"].Should().Be("func-123");
    }

    [Fact]
    public void GetCorrelationId_FallsBackFromHeadersToFunctionContext()
    {
        var requestWithTraceParent = TestHttpRequestData.Create();
        requestWithTraceParent.Headers.Add("traceparent", "00-trace-parent");

        var requestWithoutHeaders = TestHttpRequestData.Create();

        requestWithTraceParent.GetCorrelationId().Should().Be("00-trace-parent");
        requestWithoutHeaders.GetCorrelationId().Should().Be("ctx-correlation");
        requestWithoutHeaders.ToErrorContext().Method.Should().Be("GET");
    }
}
