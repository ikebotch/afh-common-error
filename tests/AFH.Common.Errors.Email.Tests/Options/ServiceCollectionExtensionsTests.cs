using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.Email.Builders;
using AFH.Common.Errors.Email.DependencyInjection;
using AFH.Common.Errors.Email.Options;
using AFH.Common.Errors.Email.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AFH.Common.Errors.Email.Tests.Options;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAfhCommonErrorsEmail_RegistersNotifierBuilderAndOptions()
    {
        var provider = new ServiceCollection()
            .AddAfhCommonErrorsEmail(
                ErrorEmailTestData.CreateOptions(),
                _ => (_, _, _) => Task.CompletedTask)
            .BuildServiceProvider();

        provider.GetService<IErrorNotifier>().Should().NotBeNull();
        provider.GetService<ErrorEmailMessageBuilder>().Should().NotBeNull();
        provider.GetService<ErrorEmailOptions>().Should().NotBeNull();
        provider.GetRequiredService<ErrorEmailOptions>().SubjectPrefix.Should().Be("[Errors]");
    }
}
