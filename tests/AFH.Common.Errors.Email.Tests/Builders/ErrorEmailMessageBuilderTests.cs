using AFH.Common.Errors.Email.Builders;
using AFH.Common.Errors.Email.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.Email.Tests.Builders;

public sealed class ErrorEmailMessageBuilderTests
{
    [Fact]
    public void BuildTemplateModel_MapsNotificationIntoTemplateModel()
    {
        var builder = new ErrorEmailMessageBuilder();

        var model = builder.BuildTemplateModel(
            ErrorEmailTestData.CreateRequest(),
            ErrorEmailTestData.CreateOptions());

        model.Subject.Should().Be("[Errors] Error: dependency.failure");
        model.TraceId.Should().Be("trace-1");
        model.CorrelationId.Should().Be("corr-1");
        model.Details.Should().HaveCount(1);
        model.FromAddress.Should().Be("errors@afh.local");
    }

    [Fact]
    public void BuildSubject_UsesConfiguredPrefixAndRecordCode()
    {
        var builder = new ErrorEmailMessageBuilder();

        var subject = builder.BuildSubject(
            ErrorEmailTestData.CreateRequest(),
            ErrorEmailTestData.CreateOptions());

        subject.Should().Be("[Errors] Error: dependency.failure");
    }

    [Fact]
    public void BuildTemplateModel_WhenDetailsAreDisabled_OmitsDetails()
    {
        var builder = new ErrorEmailMessageBuilder();

        var model = builder.BuildTemplateModel(
            ErrorEmailTestData.CreateRequest(),
            ErrorEmailTestData.CreateOptions(includeDetails: false));

        model.Details.Should().BeEmpty();
    }

    [Fact]
    public void BuildBody_CreatesReadableBodyWithSortedMetadata()
    {
        var builder = new ErrorEmailMessageBuilder();
        var request = ErrorEmailTestData.CreateRequest(
            new Dictionary<string, string?>
            {
                ["zeta"] = "last",
                ["alpha"] = "first"
            });
        var model = builder.BuildTemplateModel(request, ErrorEmailTestData.CreateOptions());

        var body = builder.BuildBody(model);

        body.Should().Contain("Summary: Dependency call failed.")
            .And.Contain("Severity: Error")
            .And.Contain("Details:")
            .And.Contain("TraceId: trace-1")
            .And.Contain("CorrelationId: corr-1");

        body.IndexOf("- alpha: first", StringComparison.Ordinal)
            .Should().BeLessThan(body.IndexOf("- zeta: last", StringComparison.Ordinal));
    }
}
