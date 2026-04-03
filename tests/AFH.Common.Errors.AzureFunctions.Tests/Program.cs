using System.Net;
using System.Security.Claims;
using AFH.Common.Errors.AzureFunctions.Builders;
using AFH.Common.Errors.AzureFunctions.DependencyInjection;
using AFH.Common.Errors.AzureFunctions.Extensions;
using AFH.Common.Errors.AzureFunctions.Mapping;
using AFH.Common.Errors.AzureFunctions.Responses;
using AFH.Common.Errors.Exceptions;
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
            MapsRequestExceptionContext,
            WritesJsonErrorResponses,
            BuildsHttpResponsesFromExceptions,
            RegistersAzureFunctionsServices
        };

        foreach (var test in tests)
        {
            test();
        }

        Console.WriteLine($"Executed {tests.Length} Azure Functions adapter tests successfully.");
        return 0;
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
    private TestHttpRequestData(FunctionContext functionContext)
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
