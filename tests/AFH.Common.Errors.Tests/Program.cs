using AFH.Common.Errors.Builders;
using AFH.Common.Errors.Codes;
using AFH.Common.Errors.Exceptions;
using AFH.Common.Errors.Mapping;
using AFH.Common.Errors.Models;

return TestRunner.Run();

internal static class TestRunner
{
    public static int Run()
    {
        var tests = new Action[]
        {
            MapsValidationExceptionsToBadRequest,
            MapsDependencyTimeoutsToGatewayTimeout,
            BuildsValidationResponsesWithValidationErrors,
            BuildsErrorRecordsFromMappings,
            BuildsNotificationRequestsFromErrorRecords
        };

        foreach (var test in tests)
        {
            test();
        }

        Console.WriteLine($"Executed {tests.Length} core tests successfully.");
        return 0;
    }

    private static void MapsValidationExceptionsToBadRequest()
    {
        var mapper = new DefaultExceptionMapper();
        var exception = new ValidationException(
        [
            new ValidationErrorDetail("email", "Email is required.", ValidationErrorCodes.Required.Value)
        ]);

        var result = mapper.Map(exception, new ErrorContext(TraceId: "trace-1"));

        Assert.Equal(400, result.StatusCode);
        Assert.Equal(ValidationErrorCodes.InvalidInput.Value, result.ErrorCode.Value);
        Assert.Equal(1, result.ValidationErrors.Count);
    }

    private static void MapsDependencyTimeoutsToGatewayTimeout()
    {
        var mapper = new DefaultExceptionMapper();
        var result = mapper.Map(new DependencyTimeoutException("Timed out."));

        Assert.Equal(504, result.StatusCode);
        Assert.Equal(DependencyErrorCodes.Timeout.Value, result.ErrorCode.Value);
    }

    private static void BuildsValidationResponsesWithValidationErrors()
    {
        var builder = new ErrorResponseBuilder(new DefaultExceptionMapper());
        var response = builder.Build(
            new ValidationException(
            [
                new ValidationErrorDetail("name", "Name is required.", ValidationErrorCodes.Required.Value)
            ]),
            new ErrorContext(TraceId: "trace-2", CorrelationId: "corr-2"));

        var validationResponse = Assert.IsType<ValidationErrorResponse>(response);

        Assert.Equal("trace-2", validationResponse.TraceId);
        Assert.Equal("corr-2", validationResponse.CorrelationId);
        Assert.Equal(1, validationResponse.ValidationErrors.Count);
        Assert.Equal(ValidationErrorCodes.InvalidInput.Value, validationResponse.Error.Code);
    }

    private static void BuildsErrorRecordsFromMappings()
    {
        var mapper = new DefaultExceptionMapper();
        var mapping = mapper.Map(new NotFoundException("Missing resource."), new ErrorContext(Path: "/errors/42"));
        var builder = new ErrorRecordBuilder();
        var record = builder.Build(mapping);

        Assert.Equal(CommonErrorCodes.NotFound.Value, record.Code);
        Assert.Equal(ErrorCategory.NotFound, record.Category);
        Assert.Equal("/errors/42", record.Context?.Path);
    }

    private static void BuildsNotificationRequestsFromErrorRecords()
    {
        var request = new NotificationRequestBuilder().Build(new ErrorRecord
        {
            Code = DependencyErrorCodes.Failure.Value,
            Category = ErrorCategory.Dependency,
            Severity = ErrorSeverity.Error,
            Message = "Dependency failed."
        });

        Assert.Equal("Error: dependency.failure", request.Subject);
        Assert.Equal("Dependency failed.", request.Summary);
        Assert.Equal(ErrorSeverity.Error, request.Severity);
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

    public static T IsType<T>(object value)
    {
        if (value is not T typed)
        {
            throw new InvalidOperationException($"Expected value of type '{typeof(T).Name}' but found '{value.GetType().Name}'.");
        }

        return typed;
    }
}
