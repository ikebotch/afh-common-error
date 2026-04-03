using System.Text;
using AFH.Common.Errors.AzureFunctions.Builders;
using AFH.Common.Errors.AzureFunctions.DependencyInjection;
using AFH.Common.Errors.AzureFunctions.Tests.Fixtures;
using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AFH.Common.Errors.AzureFunctions.Tests.Builders;

public sealed class AzureFunctionErrorResponseBuilderTests
{
    [Fact]
    public async Task BuildAsync_WithRequestAndException_ReturnsExpectedResponse()
    {
        var services = new ServiceCollection()
            .AddAfhCommonErrorsAzureFunctions()
            .BuildServiceProvider();

        var builder = services.GetRequiredService<AzureFunctionErrorResponseBuilder>();
        var request = TestHttpRequestData.Create();

        var response = await builder.BuildAsync(request, new UnauthorizedException("Denied."));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        (await ReadBodyAsync(response)).Should().Contain("\"code\":\"common.unauthorized\"");
    }

    [Fact]
    public async Task BuildAsync_WithContextAndValidationException_WritesValidationPayload()
    {
        var services = new ServiceCollection()
            .AddAfhCommonErrorsAzureFunctions()
            .BuildServiceProvider();

        var builder = services.GetRequiredService<AzureFunctionErrorResponseBuilder>();
        var context = new TestFunctionContext();
        var request = new TestHttpRequestData(context);
        request.Headers.Add("x-correlation-id", "req-correlation");

        var response = await builder.BuildAsync(
            context,
            request,
            new ValidationException(
            [
                new("email", "Required.", ValidationErrorCodes.Required.Value)
            ]));

        var payload = await ReadBodyAsync(response);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        payload.Should().Contain("\"validationErrors\"");
        payload.Should().Contain("\"correlationId\":\"req-correlation\"");
    }

    [Fact]
    public void AddAfhCommonErrorsAzureFunctions_RegistersBuilderDependencies()
    {
        var provider = new ServiceCollection()
            .AddAfhCommonErrorsAzureFunctions()
            .BuildServiceProvider();

        provider.GetService<AzureFunctionErrorResponseBuilder>().Should().NotBeNull();
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
