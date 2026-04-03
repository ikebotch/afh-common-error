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
            BuildsSubjectUsingConfiguredPrefixAndRecordCode,
            OmitsDetailsWhenConfiguredToDoSo,
            BuildsReadableBodyFromTemplateModel,
            SortsMetadataInBodyOutput,
            NotifierInvokesSenderWithBuiltModelAndBody,
            PassesCancellationTokenToSender,
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
        Assert.Equal("errors@afh.local", model.FromAddress);
    }

    private static void BuildsSubjectUsingConfiguredPrefixAndRecordCode()
    {
        var builder = new ErrorEmailMessageBuilder();
        var subject = builder.BuildSubject(CreateRequest(), CreateOptions());

        Assert.Equal("[Errors] Error: dependency.failure", subject);
    }

    private static void OmitsDetailsWhenConfiguredToDoSo()
    {
        var builder = new ErrorEmailMessageBuilder();
        var options = CreateOptions();
        options = new ErrorEmailOptions
        {
            FromAddress = options.FromAddress,
            FromDisplayName = options.FromDisplayName,
            ToAddresses = options.ToAddresses,
            CcAddresses = options.CcAddresses,
            BccAddresses = options.BccAddresses,
            SubjectPrefix = options.SubjectPrefix,
            IncludeDetails = false
        };
        var model = builder.BuildTemplateModel(
            CreateRequest(),
            options);

        Assert.Equal(0, model.Details.Count);
    }

    private static void BuildsReadableBodyFromTemplateModel()
    {
        var builder = new ErrorEmailMessageBuilder();
        var model = builder.BuildTemplateModel(CreateRequest(), CreateOptions());
        var body = builder.BuildBody(model);

        Assert.Contains("Summary: Dependency call failed.", body);
        Assert.Contains("Severity: Error", body);
        Assert.Contains("Details:", body);
        Assert.Contains("Metadata:", body);
        Assert.Contains("TraceId: trace-1", body);
        Assert.Contains("CorrelationId: corr-1", body);
    }

    private static void SortsMetadataInBodyOutput()
    {
        var builder = new ErrorEmailMessageBuilder();
        var request = CreateRequest(new Dictionary<string, string?>
        {
            ["zeta"] = "last",
            ["alpha"] = "first"
        });
        var body = builder.BuildBody(builder.BuildTemplateModel(request, CreateOptions()));

        Assert.True(body.IndexOf("- alpha: first", StringComparison.Ordinal) < body.IndexOf("- zeta: last", StringComparison.Ordinal));
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

    private static void PassesCancellationTokenToSender()
    {
        var expected = new CancellationToken(canceled: true);
        CancellationToken actual = default;

        var notifier = new AFH.Common.Errors.Email.Notifications.EmailErrorNotifier(
            CreateOptions(),
            new ErrorEmailMessageBuilder(),
            (_, _, cancellationToken) =>
            {
                actual = cancellationToken;
                return Task.CompletedTask;
            });

        notifier.NotifyAsync(CreateRequest(), expected).GetAwaiter().GetResult();

        Assert.Equal(expected, actual);
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
        var options = provider.GetService<ErrorEmailOptions>();

        Assert.NotNull(notifier);
        Assert.NotNull(builder);
        Assert.NotNull(options);
        Assert.Equal("[Errors]", options!.SubjectPrefix);
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

    private static ErrorNotificationRequest CreateRequest(IReadOnlyDictionary<string, string?>? metadata = null)
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
            Metadata = metadata ?? new Dictionary<string, string?> { ["traceId"] = "trace-1" }
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

    public static void True(bool condition)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Expected condition to be true.");
        }
    }
}
