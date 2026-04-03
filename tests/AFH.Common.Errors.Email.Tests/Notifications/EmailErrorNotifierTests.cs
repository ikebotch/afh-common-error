using AFH.Common.Errors.Email.Builders;
using AFH.Common.Errors.Email.Models;
using AFH.Common.Errors.Email.Notifications;
using AFH.Common.Errors.Email.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace AFH.Common.Errors.Email.Tests.Notifications;

public sealed class EmailErrorNotifierTests
{
    [Fact]
    public async Task NotifyAsync_BuildsTemplateAndInvokesSender()
    {
        ErrorEmailTemplateModel? capturedModel = null;
        string? capturedBody = null;

        var notifier = new EmailErrorNotifier(
            ErrorEmailTestData.CreateOptions(),
            new ErrorEmailMessageBuilder(),
            (model, body, _) =>
            {
                capturedModel = model;
                capturedBody = body;
                return Task.CompletedTask;
            });

        await notifier.NotifyAsync(ErrorEmailTestData.CreateRequest());

        capturedModel.Should().NotBeNull();
        capturedBody.Should().NotBeNull();
        capturedModel!.Subject.Should().Be("[Errors] Error: dependency.failure");
        capturedBody.Should().Contain("Dependency call failed.");
    }

    [Fact]
    public async Task NotifyAsync_PassesCancellationTokenToSender()
    {
        var expected = new CancellationToken(canceled: true);
        var actual = CancellationToken.None;

        var notifier = new EmailErrorNotifier(
            ErrorEmailTestData.CreateOptions(),
            new ErrorEmailMessageBuilder(),
            (_, _, cancellationToken) =>
            {
                actual = cancellationToken;
                return Task.CompletedTask;
            });

        await notifier.NotifyAsync(ErrorEmailTestData.CreateRequest(), expected);

        actual.Should().Be(expected);
    }
}
