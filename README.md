# AFH.Common.Errors SDK

Shared error handling SDK for AFH services. It standardizes how errors are modeled, mapped, persisted, and communicated across services while keeping service-specific behavior local.

## Packages

- `AFH.Common.Errors`: core abstractions, models, and builders for error handling.
- `AFH.Common.Errors.AzureFunctions`: Azure Functions response writer and exception mapping adapter.
- `AFH.Common.Errors.ApplicationInsights`: telemetry mapping/enrichment adapter for shared error records.
- `AFH.Common.Errors.Email`: provider-agnostic email notification adapter for error notifications.
- `AFH.Common.Errors.EntityFramework`: persistence adapter for storing error records via EF Core.

## What AFH.Common.Errors Does

`AFH.Common.Errors` defines the shared error model and the primitives used by adapters and services:

- Core error codes, categories, severities, and base exception types.
- Mapping and builder utilities to turn exceptions into `ErrorRecord` and `ErrorResponse`.
- Abstractions for response building, notification, and persistence.

The SDK is intentionally small. It provides the model and adapters, but leaves service-specific context and business rules in each service.

## What AFH.Common.Errors.AzureFunctions Does

The Azure Functions adapter wires up the core error mapping and response building for Functions. It provides:

- Azure Functions-specific exception mapping for HTTP responses.
- A response writer that converts errors into consistent HTTP responses.
- Registration for the core mappers and builders required by Functions.

## What Must Stay Local To Each Service

These responsibilities are owned by the consuming service and must not be centralized:

- Mapping local exception types to shared error codes.
- Capturing service-specific context (tenant, operation names, request metadata).
- Choosing when to notify or persist based on service policies.
- Any routing, escalation, or recipient logic for notifications.

## Recommended Registration Pattern (Program.cs)

Register the shared SDK and any adapters in your composition root. Keep service-specific pieces close to the service.

```csharp
builder.Services
    .AddAfhCommonErrorsAzureFunctions()
    .AddAfhCommonErrorsApplicationInsights()
    .AddAfhCommonErrorsEntityFramework<MyDbContext>()
    .AddAfhCommonErrorsEmail(
        new ErrorEmailOptions
        {
            FromAddress = "errors@service.local",
            ToAddresses = ["ops@service.local"]
        },
        _ => (model, body, ct) => EmailSender.SendAsync(model, body, ct));
```

## Recommended Local Exception Mapper Pattern

Define service-local mappings for exceptions so shared packages remain generic.

```csharp
public sealed class ServiceExceptionMapper : IExceptionMapper
{
    public ExceptionMappingResult Map(Exception exception)
    {
        return exception switch
        {
            ServiceUnavailableException => ExceptionMappingResult.DependencyFailure("service.unavailable"),
            CustomerNotFoundException => ExceptionMappingResult.NotFound("customer.not_found"),
            _ => ExceptionMappingResult.Unexpected()
        };
    }
}
```

## Error Flow Through The Shared Response Writer

1. A service catches an exception and passes it to the shared mapping pipeline.
2. The mapper returns an `ErrorRecord` with code, severity, category, and context.
3. The response builder creates a standardized `ErrorResponse`.
4. The response writer serializes the response into the transport (HTTP for Functions).
5. Optional adapters persist or notify based on the `ErrorRecord`.

## When To Use The Adapters

- ApplicationInsights: map `ErrorRecord` to telemetry properties and metrics for observability.
- Email: send provider-agnostic notifications for critical error events.
- EntityFramework: persist `ErrorRecord` for audit and diagnostics.

These adapters are optional and should be enabled only when a service needs them.

## What Not To Centralize

- Service-specific error codes, routing rules, or escalation policies.
- Service-specific schema extensions or database conventions.
- Email recipient lists or environment-specific notification behavior.

## Concise Example Integration

```csharp
try
{
    await handler.ExecuteAsync(request, cancellationToken);
}
catch (Exception exception)
{
    var mapping = exceptionMapper.Map(exception);
    var record = errorRecordBuilder.Build(mapping, contextAccessor.Get());
    await persistenceWriter.WriteAsync(record, cancellationToken);
    await notifier.NotifyAsync(new ErrorNotificationRequest
    {
        Subject = $"Error: {record.Code}",
        Summary = record.Message,
        Severity = record.Severity,
        Record = record
    }, cancellationToken);
    throw;
}
```

## Concise Example Local Exception Mapper

```csharp
builder.Services.AddSingleton<IExceptionMapper, ServiceExceptionMapper>();
```
