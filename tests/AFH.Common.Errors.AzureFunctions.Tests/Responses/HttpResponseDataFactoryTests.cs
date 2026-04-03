using AFH.Common.Errors.AzureFunctions.Responses;
using AFH.Common.Errors.AzureFunctions.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.AzureFunctions.Tests.Responses;

public sealed class HttpResponseDataFactoryTests
{
    [Fact]
    public void Create_SetsExpectedStatusCode()
    {
        var request = TestHttpRequestData.Create();
        var factory = new HttpResponseDataFactory();

        var response = factory.Create(request, System.Net.HttpStatusCode.BadGateway);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadGateway);
    }
}
