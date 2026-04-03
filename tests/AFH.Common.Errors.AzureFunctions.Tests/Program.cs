using System.Net;
using System.Security.Claims;
using System.Text;
using AFH.Common.Errors.AzureFunctions.Builders;
using AFH.Common.Errors.AzureFunctions.DependencyInjection;
using AFH.Common.Errors.AzureFunctions.Extensions;
using AFH.Common.Errors.AzureFunctions.Mapping;
using AFH.Common.Errors.AzureFunctions.Responses;
using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Exceptions;
using AFH.Common.Errors.Mapping;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

return TestRunner.Run();

internal static class TestRunner
{
    public static int Run()
    {
        var tests = new Action[]
        {
            MapsFunctionContextIntoErrorContext,
            MapsRequestExceptionContext,
            FallsBackToTraceParentHeaderThenContextCorrelationId,
            MergesFunctionAndRequestContext,
            ResolvesHttpStatusCodes,
            CreatesResponsesWithExpectedStatus,
            WritesJsonErrorResponses,
            BuildsHttpResponsesFromExceptions,
            BuildsHttpResponsesFromMergedContext,
            RegistersAzureFunctionsServices
        };

        foreach (var test in tests)
        {
            test();
        }

        Console.WriteLine($"Executed {tests.Length} Azure Functions adapter tests successfully.");
        return 0;
    }

    private static void MapsFunctionContextIntoErrorContext()
    {
        var context = new TestFunctionContext();
        context.Items["CorrelationId"] = "ctx-correlation";

        var errorContext = context.ToErrorContext();

        Assert.Equal("inv-123", errorContext.TraceId);
        Assert.Equal("ctx-correlation", errorContext.CorrelationId);
        Assert.Equal("ErrorsFunction", errorContext.Operation);
        Assert.Equal("func-123", errorContext.Metadata?["functionId"]);
    }

    private static void MapsRequestExceptionContext()
    {
        var request = TestHttpRequestData.Create();
        request.Headers.Add("x-correlation-id", "corr-123");
        var mapper = new AzureFunctionExceptionMapper(new DefaultExceptionMapper());

        var result = mapper.Map(new NotFoundException("Missing."), request);

        Assert.Equal(404, result.StatusCode);
        Assert.Equal("corr-123", result.Context?.CorrelationId);
        Assert.Equal("/clients/42", result.Context?.Path);
        Assert.Equal("GET", result.Context?.Method);
        Assert.Equal("localhost", result.Context?.Metadata?["host"]);
    }

    private static void FallsBackToTraceParentHeaderThenContextCorrelationId()
    {
        var requestWithTraceParent = TestHttpRequestData.Create();
        requestWithTraceParent.Headers.Add("traceparent", "00-trace-parent");

        var requestWithoutHeaders = TestHttpRequestData.Create();

        Assert.Equal("00-trace-parent", requestWithTraceParent.GetCorrelationId());
        Assert.Equal("ctx-correlation", requestWithoutHeaders.GetCorrelationId());
        Assert.Equal("GET", requestWithoutHeaders.ToErrorContext().Method);
    }

    private static void MergesFunctionAndRequestContext()
    {
        var mapper = new AzureFunctionExceptionMapper(new DefaultExceptionMapper());
        var context = new TestFunctionContext();
        context.Items["CorrelationId"] = "ctx-correlation";

        var request = new TestHttpRequestData(context);
        request.Headers.Add("x-correlation-id", "req-correlation");

        var result = mapper.Map(new DependencyTimeoutException("Timed out."), context, request);

        Assert.Equal(504, result.StatusCode);
        Assert.Equal("req-correlation", result.Context?.CorrelationId);
        Assert.Equal("ErrorsFunction", result.Context?.Operation);
        Assert.Equal("/clients/42", result.Context?.Path);
    }

    private static void ResolvesHttpStatusCodes()
    {
        var mapping = new DefaultExceptionMapper().Map(new ForbiddenException("Denied."));

        Assert.Equal(HttpStatusCode.Forbidden, HttpStatusCodeResolver.Resolve(CommonErrorCodes.Forbidden));
        Assert.Equal(HttpStatusCode.Forbidden, HttpStatusCodeResolver.Resolve(mapping));
    }

    private static void CreatesResponsesWithExpectedStatus()
    {
        var request = TestHttpRequestData.Create();
        var factory = new HttpResponseDataFactory();
        var response = factory.Create(request, HttpStatusCode.BadGateway);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    private static void WritesJsonErrorResponses()
    {
        var request = TestHttpRequestData.Create();
        var factory = new HttpResponseDataFactory();
        var response = factory.Create(request, HttpStatusCode.BadRequest);
        var writer = new FunctionErrorResponseWriter();

        writer.WriteAsync(
            response,
            new AFH.Common.Errors.Models.ErrorResponse
            {
                StatusCode = 400,
                Error = new AFH.Common.Errors.Models.ErrorDetail("validation.invalid_input", "Invalid.")
            }).GetAwaiter().GetResult();

        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        var content = reader.ReadToEnd();

        Assert.Contains("\"code\":\"validation.invalid_input\"", content);
        Assert.Contains("\"statusCode\":400", content);
        Assert.Equal("application/json; charset=utf-8", response.Headers.GetValues("Content-Type").Single());
    }

    private static void BuildsHttpResponsesFromExceptions()
    {
        var services = new ServiceCollection()
            .AddAfhCommonErrorsAzureFunctions()
            .BuildServiceProvider();

        var builder = services.GetRequiredService<AzureFunctionErrorResponseBuilder>();
        var request = TestHttpRequestData.Create();
        var response = builder.BuildAsync(request, new UnauthorizedException("Denied.")).GetAwaiter().GetResult();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        AssertContainsResponsePayload(response, "\"code\":\"common.unauthorized\"");
    }

    private static void BuildsHttpResponsesFromMergedContext()
    {
        var services = new ServiceCollection()
            .AddAfhCommonErrorsAzureFunctions()
            .BuildServiceProvider();

        var builder = services.GetRequiredService<AzureFunctionErrorResponseBuilder>();
        var context = new TestFunctionContext();
        var request = new TestHttpRequestData(context);
        request.Headers.Add("x-correlation-id", "req-correlation");

        var response = builder.BuildAsync(context, request, new ValidationException(
        [
            new AFH.Common.Errors.Models.ValidationErrorDetail("email", "Required.", ValidationErrorCodes.Required.Value)
        ])).GetAwaiter().GetResult();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertContainsResponsePayload(response, "\"validationErrors\"");
        AssertContainsResponsePayload(response, "\"correlationId\":\"req-correlation\"");
    }

    private static void RegistersAzureFunctionsServices()
    {
        var services = new ServiceCollection();
        services.AddAfhCommonErrorsAzureFunctions();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<AzureFunctionExceptionMapper>());
        Assert.NotNull(provider.GetService<HttpResponseDataFactory>());
        Assert.NotNull(provider.GetService<AzureFunctionErrorResponseBuilder>());
    }

    private static void AssertContainsResponsePayload(HttpResponseData response, string expected)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var content = reader.ReadToEnd();
        Assert.Contains(expected, content);
        response.Body.Position = 0;
    }
}

internal sealed class TestFunctionContext : FunctionContext
{
    private readonly Dictionary<object, object> _items = [];

    public override string InvocationId => "inv-123";

    public override string FunctionId => "func-123";

    public override TraceContext TraceContext => throw new NotSupportedException();

    public override BindingContext BindingContext => throw new NotSupportedException();

    public override RetryContext RetryContext => null!;

    public override IServiceProvider InstanceServices { get; set; } = new ServiceCollection().BuildServiceProvider();

    public override FunctionDefinition FunctionDefinition { get; } = new TestFunctionDefinition();

    public override IDictionary<object, object> Items
    {
        get => _items;
        set
        {
            _items.Clear();

            foreach (var pair in value)
            {
                _items[pair.Key] = pair.Value;
            }
        }
    }

    public override IInvocationFeatures Features => throw new NotSupportedException();

    public override CancellationToken CancellationToken => CancellationToken.None;
}

internal sealed class TestFunctionDefinition : FunctionDefinition
{
    public override string PathToAssembly => string.Empty;

    public override string EntryPoint => string.Empty;

    public override string Id => "func-123";

    public override string Name => "ErrorsFunction";

    public override IReadOnlyDictionary<string, BindingMetadata> InputBindings => new Dictionary<string, BindingMetadata>();

    public override IReadOnlyDictionary<string, BindingMetadata> OutputBindings => new Dictionary<string, BindingMetadata>();

    public override IEnumerable<FunctionParameter> Parameters => [];
}

internal sealed class TestHttpRequestData : HttpRequestData
{
    public TestHttpRequestData(FunctionContext functionContext)
        : base(functionContext)
    {
    }

    public override Stream Body { get; } = new MemoryStream();

    public override HttpHeadersCollection Headers { get; } = [];

    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = [];

    public override Uri Url { get; } = new("https://localhost/clients/42");

    public override IEnumerable<ClaimsIdentity> Identities { get; } = [];

    public override string Method { get; } = "GET";

    public override HttpResponseData CreateResponse()
    {
        return new TestHttpResponseData(FunctionContext);
    }

    public static TestHttpRequestData Create()
    {
        var context = new TestFunctionContext();
        context.Items["CorrelationId"] = "ctx-correlation";
        return new TestHttpRequestData(context);
    }
}

internal sealed class TestHttpResponseData : HttpResponseData
{
    public TestHttpResponseData(FunctionContext functionContext)
        : base(functionContext)
    {
    }

    public override HttpStatusCode StatusCode { get; set; }

    public override HttpHeadersCollection Headers { get; set; } = [];

    public override Stream Body { get; set; } = new MemoryStream();

    public override HttpCookies Cookies { get; } = new();
}

internal static class Assert
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}' but found '{actual}'.");
        }
    }

    public static void Contains(string expected, string actual)
    {
        if (!actual.Contains(expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected to find '{expected}' in '{actual}'.");
        }
    }

    public static void NotNull(object? value)
    {
        if (value is null)
        {
            throw new InvalidOperationException("Expected value to be non-null.");
        }
    }
}
