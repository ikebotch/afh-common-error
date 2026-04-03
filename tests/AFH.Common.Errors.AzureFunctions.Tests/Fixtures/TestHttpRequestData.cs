using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AFH.Common.Errors.AzureFunctions.Tests.Fixtures;

internal sealed class TestHttpRequestData : HttpRequestData
{
    public TestHttpRequestData(
        FunctionContext functionContext,
        Uri? url = null,
        string method = "GET")
        : base(functionContext)
    {
        Url = url ?? new Uri("https://localhost/clients/42");
        Method = method;
    }

    public override Stream Body { get; } = new MemoryStream();

    public override HttpHeadersCollection Headers { get; } = [];

    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = [];

    public override Uri Url { get; }

    public override IEnumerable<ClaimsIdentity> Identities { get; } = [];

    public override string Method { get; }

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
