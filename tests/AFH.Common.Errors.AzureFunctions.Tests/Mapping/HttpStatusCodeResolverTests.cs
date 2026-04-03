using AFH.Common.Errors.AzureFunctions.Mapping;
using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Exceptions;
using AFH.Common.Errors.Mapping;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.AzureFunctions.Tests.Mapping;

public sealed class HttpStatusCodeResolverTests
{
    [Fact]
    public void Resolve_WithErrorCode_MapsToHttpStatusCode()
    {
        HttpStatusCodeResolver.Resolve(CommonErrorCodes.Forbidden)
            .Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public void Resolve_WithMappingResult_UsesMappedStatusCode()
    {
        var mapping = new DefaultExceptionMapper().Map(new ForbiddenException("Denied."));

        HttpStatusCodeResolver.Resolve(mapping)
            .Should().Be(System.Net.HttpStatusCode.Forbidden);
    }
}
