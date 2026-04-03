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
            ExposesStableCodeCatalogValues,
            UsesErrorCodeValueForStringRepresentation,
            AppliesDefaultMessagesForTypedExceptions,
            PreservesMetadataOnAfhException,
            MapsValidationExceptionsToBadRequest,
            MapsKeyNotFoundExceptionsToNotFound,
            MapsUnknownExceptionsToUnexpected,
            ResolvesKnownStatusCodes,
            BuildsValidationResponsesWithValidationErrors,
            BuildsNonValidationResponsesWithContext,
            BuildsValidationMappingDetailsFromErrors,
            BuildsErrorRecordsFromMappings,
            BuildsNotificationRequestsFromErrorRecords,
            PreservesExplicitRecordTimestamp
        };

        foreach (var test in tests)
        {
            test();
        }

        Console.WriteLine($"Executed {tests.Length} core tests successfully.");
        return 0;
    }

    private static void ExposesStableCodeCatalogValues()
    {
        Assert.Equal("common.unexpected", CommonErrorCodes.Unexpected.Value);
        Assert.Equal(ErrorSeverity.Warning, ValidationErrorCodes.Required.Severity);
        Assert.Equal(ErrorCategory.Authorization, AuthErrorCodes.InvalidCredentials.Category);
        Assert.Equal("dependency.timeout", DependencyErrorCodes.Timeout.Value);
        Assert.Equal(ErrorCategory.Persistence, PersistenceErrorCodes.WriteFailed.Category);
    }

    private static void UsesErrorCodeValueForStringRepresentation()
    {
        Assert.Equal("validation.invalid_input", ValidationErrorCodes.InvalidInput.ToString());
    }

    private static void AppliesDefaultMessagesForTypedExceptions()
    {
        var notFound = new NotFoundException();
        var forbidden = new ForbiddenException();
        var concurrency = new ConcurrencyConflictException();

        Assert.Equal(CommonErrorCodes.NotFound.DefaultMessage, notFound.Message);
        Assert.Equal(AuthErrorCodes.Forbidden.DefaultMessage, forbidden.Message);
        Assert.Equal(PersistenceErrorCodes.ConcurrencyConflict.DefaultMessage, concurrency.Message);
    }

    private static void PreservesMetadataOnAfhException()
    {
        var exception = new AfhException(
            DependencyErrorCodes.Unavailable,
            metadata: new Dictionary<string, string?> { ["region"] = "uksouth" });

        Assert.Equal("uksouth", exception.Metadata["region"]);
        Assert.Equal(DependencyErrorCodes.Unavailable.DefaultMessage, exception.Message);
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
        Assert.Equal("email", result.ValidationErrors[0].Field);
    }

    private static void MapsKeyNotFoundExceptionsToNotFound()
    {
        var mapper = new DefaultExceptionMapper();
        var result = mapper.Map(new KeyNotFoundException("Missing."));

        Assert.Equal(404, result.StatusCode);
        Assert.Equal(CommonErrorCodes.NotFound.Value, result.ErrorCode.Value);
        Assert.Equal("Missing.", result.Message);
    }

    private static void MapsUnknownExceptionsToUnexpected()
    {
        var mapper = new DefaultExceptionMapper();
        var result = mapper.Map(new InvalidOperationException("Boom."));

        Assert.Equal(500, result.StatusCode);
        Assert.Equal(CommonErrorCodes.Unexpected.Value, result.ErrorCode.Value);
        Assert.Empty(result.Details);
    }

    private static void ResolvesKnownStatusCodes()
    {
        Assert.Equal(403, ErrorStatusCodeResolver.Resolve(AuthErrorCodes.Forbidden));
        Assert.Equal(401, ErrorStatusCodeResolver.Resolve(CommonErrorCodes.Unauthorized));
        Assert.Equal(409, ErrorStatusCodeResolver.Resolve(PersistenceErrorCodes.ConcurrencyConflict));
        Assert.Equal(504, ErrorStatusCodeResolver.Resolve(DependencyErrorCodes.Timeout));
        Assert.Equal(500, ErrorStatusCodeResolver.Resolve(CommonErrorCodes.Unexpected));
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
        Assert.Equal("name", validationResponse.Details[0].Target);
    }

    private static void BuildsNonValidationResponsesWithContext()
    {
        var builder = new ErrorResponseBuilder(new DefaultExceptionMapper());
        var response = builder.Build(
            new UnauthorizedException("Denied."),
            new ErrorContext(TraceId: "trace-3", CorrelationId: "corr-3"));

        Assert.Equal(401, response.StatusCode);
        Assert.Equal("trace-3", response.TraceId);
        Assert.Equal("corr-3", response.CorrelationId);
        Assert.Equal(AuthErrorCodes.Unauthorized.Value, response.Error.Code);
    }

    private static void BuildsValidationMappingDetailsFromErrors()
    {
        var mapper = new ValidationExceptionMapper();
        var result = mapper.Map(
            new ValidationException(
            [
                new ValidationErrorDetail("email", "Email is required.", ValidationErrorCodes.Required.Value),
                new ValidationErrorDetail("age", "Age is invalid.")
            ]));

        Assert.Equal(2, result.Details.Count);
        Assert.Equal(ValidationErrorCodes.Required.Value, result.Details[0].Code);
        Assert.Equal("email", result.Details[0].Target);
        Assert.Equal(ValidationErrorCodes.InvalidInput.Value, result.Details[1].Code);
    }

    private static void BuildsErrorRecordsFromMappings()
    {
        var mapper = new DefaultExceptionMapper();
        var mapping = mapper.Map(new NotFoundException("Missing resource."), new ErrorContext(Path: "/errors/42"));
        var builder = new ErrorRecordBuilder();
        var record = builder.Build(mapping);

        Assert.Equal(CommonErrorCodes.NotFound.Value, record.Code);
        Assert.Equal(ErrorCategory.NotFound, record.Category);
        Assert.Equal(ErrorSeverity.Warning, record.Severity);
        Assert.Equal("/errors/42", record.Context?.Path);
        Assert.Equal(typeof(NotFoundException).FullName, record.ExceptionType);
    }

    private static void BuildsNotificationRequestsFromErrorRecords()
    {
        var metadata = new Dictionary<string, string?> { ["traceId"] = "trace-9" };
        var request = new NotificationRequestBuilder().Build(new ErrorRecord
        {
            Code = DependencyErrorCodes.Failure.Value,
            Category = ErrorCategory.Dependency,
            Severity = ErrorSeverity.Error,
            Message = "Dependency failed.",
            Context = new ErrorContext(Metadata: metadata)
        });

        Assert.Equal("Error: dependency.failure", request.Subject);
        Assert.Equal("Dependency failed.", request.Summary);
        Assert.Equal(ErrorSeverity.Error, request.Severity);
        Assert.Same(metadata, request.Metadata);
    }

    private static void PreservesExplicitRecordTimestamp()
    {
        var timestamp = new DateTimeOffset(2026, 4, 3, 12, 30, 0, TimeSpan.Zero);
        var record = new ErrorRecord
        {
            Code = CommonErrorCodes.Conflict.Value,
            Category = ErrorCategory.Conflict,
            Severity = ErrorSeverity.Warning,
            Message = "Conflict.",
            OccurredUtc = timestamp
        };

        Assert.Equal(timestamp, record.OccurredUtc);
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

    public static void Empty<T>(IReadOnlyCollection<T> values)
    {
        if (values.Count != 0)
        {
            throw new InvalidOperationException($"Expected an empty collection but found {values.Count} item(s).");
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

    public static void Same(object expected, object actual)
    {
        if (!ReferenceEquals(expected, actual))
        {
            throw new InvalidOperationException("Expected both values to reference the same instance.");
        }
    }
}
