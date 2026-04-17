using System.Text;
using AFH.Common.Errors.AzureFunctions.Responses;
using AFH.Common.Errors.AzureFunctions.Tests.Fixtures;
using AFH.Common.Errors.Models;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.AzureFunctions.Tests.Responses;

public sealed class FunctionErrorResponseWriterTests
{
    [Fact]
    public async Task WriteAsync_SetsJsonContentTypeAndWritesPayload()
    {
        var response = new TestHttpResponseData(new TestFunctionContext());
        var writer = new FunctionErrorResponseWriter();

        await writer.WriteAsync(
            response,
            new ErrorResponse
            {
                StatusCode = 400,
                Error = new ErrorDetail("validation.invalid_input", "Invalid.")
            });

        response.Headers.GetValues("Content-Type").Single().Should().Be("application/json; charset=utf-8");
        (await ReadBodyAsync(response)).Should().Contain("\"statusCode\":400").And.Contain("\"code\":\"validation.invalid_input\"");
    }

    [Fact]
    public async Task WriteAsync_ReplacesExistingContentTypeHeader()
    {
        var response = new TestHttpResponseData(new TestFunctionContext());
        response.Headers.Add("Content-Type", "text/plain");

        var writer = new FunctionErrorResponseWriter();

        await writer.WriteAsync(
            response,
            new ErrorResponse
            {
                StatusCode = 503,
                Error = new ErrorDetail("dependency.unavailable", "Downstream failed.")
            });

        response.Headers.GetValues("Content-Type").Should().ContainSingle().Which.Should().Be("application/json; charset=utf-8");
    }

    private static async Task<string> ReadBodyAsync(Microsoft.Azure.Functions.Worker.Http.HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var payload = await reader.ReadToEndAsync();
        response.Body.Position = 0;
        return payload;
    }
}
