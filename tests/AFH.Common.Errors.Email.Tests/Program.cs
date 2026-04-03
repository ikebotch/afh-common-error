using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Email.Builders;
using AFH.Common.Errors.Email.DependencyInjection;
using AFH.Common.Errors.Email.Models;
using AFH.Common.Errors.Email.Options;
using AFH.Common.Errors.Models;
using Microsoft.Extensions.DependencyInjection;

return TestRunner.Run();

internal static class TestRunner
{
    public static int Run()
    {
        var tests = new Action[]
        {
            BuildsTemplateModelFromNotificationRequest,
            BuildsReadableBodyFromTemplateModel,
            NotifierInvokesSenderWithBuiltModelAndBody,
            RegistersEmailNotifierInDependencyInjection
        };

        foreach (var test in tests)
        {
            test();
        }

        Console.WriteLine($"Executed {tests.Length} email adapter tests successfully.");
        return 0;
    }

    private static void BuildsTemplateModelFromNotificationRequest()
    {
        var builder = new ErrorEmailMessageBuilder();
        var options = CreateOptions();
        var request = CreateRequest();

        var model = builder.BuildTemplateModel(request, options);

        Assert.Equal("[Errors] Error: dependency.failure", model.Subject);
        Assert.Equal("trace-1", model.TraceId);
        Assert.Equal("corr-1", model.CorrelationId);
        Assert.Equal(1, model.Details.Count);
        Assert.Equal(1, model.ToAddresses.Count);
    }

    private static void BuildsReadableBodyFromTemplateModel()
    {
        var builder = new ErrorEmailMessageBuilder();
        var model = builder.BuildTemplateModel(CreateRequest(), CreateOptions());
        var body = builder.BuildBody(model);

        Assert.Contains("Summary: Dependency call failed.", body);
        Assert.Contains("Severity: Error", body);
        Assert.Contains("Metadata:", body);
        Assert.Contains("traceId: trace-1", body);
    }

    private static void NotifierInvokesSenderWithBuiltModelAndBody()
    {
        ErrorEmailTemplateModel? capturedModel = null;
        string? capturedBody = null;

        var notifier = new AFH.Common.Errors.Email.Notifications.EmailErrorNotifier(
            CreateOptions(),
            new ErrorEmailMessageBuilder(),
            (model, body, _) =>
            {
                capturedModel = model;
                capturedBody = body;
                return Task.CompletedTask;
            });

        notifier.NotifyAsync(CreateRequest()).GetAwaiter().GetResult();

        Assert.NotNull(capturedModel);
        Assert.NotNull(capturedBody);
        Assert.Equal("[Errors] Error: dependency.failure", capturedModel!.Subject);
        Assert.Contains("Dependency call failed.", capturedBody!);
    }

    private static void RegistersEmailNotifierInDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddAfhCommonErrorsEmail(
            CreateOptions(),
            _ => (_, _, _) => Task.CompletedTask);

        var provider = services.BuildServiceProvider();
        var notifier = provider.GetService<IErrorNotifier>();
        var builder = provider.GetService<ErrorEmailMessageBuilder>();

        Assert.NotNull(notifier);
        Assert.NotNull(builder);
    }

    private static ErrorEmailOptions CreateOptions()
    {
        return new ErrorEmailOptions
        {
            FromAddress = "errors@afh.local",
            FromDisplayName = "AFH Errors",
            ToAddresses = ["ops@afh.local"],
            SubjectPrefix = "[Errors]"
        };
    }

    private static ErrorNotificationRequest CreateRequest()
    {
        return new ErrorNotificationRequest
        {
            Subject = "Error: dependency.failure",
            Summary = "Dependency call failed.",
            Severity = ErrorSeverity.Error,
            Record = new ErrorRecord
            {
                Code = DependencyErrorCodes.Failure.Value,
                Category = ErrorCategory.Dependency,
                Severity = ErrorSeverity.Error,
                Message = "Dependency call failed.",
                ExceptionType = typeof(TimeoutException).FullName,
                OccurredUtc = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero),
                Context = new ErrorContext(
                    TraceId: "trace-1",
                    CorrelationId: "corr-1",
                    Metadata: new Dictionary<string, string?> { ["traceId"] = "trace-1" }),
                Details =
                [
                    new ErrorDetail(DependencyErrorCodes.Failure.Value, "Dependency call failed.")
                ]
            },
            Metadata = new Dictionary<string, string?> { ["traceId"] = "trace-1" }
        };
    }
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
