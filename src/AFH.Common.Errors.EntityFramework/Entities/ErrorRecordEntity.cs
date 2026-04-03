namespace AFH.Common.Errors.EntityFramework.Entities;

public sealed class ErrorRecordEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? ExceptionType { get; set; }

    public string? StackTrace { get; set; }

    public DateTimeOffset OccurredUtc { get; set; }

    public string? TraceId { get; set; }

    public string? CorrelationId { get; set; }

    public string? Path { get; set; }

    public string? Method { get; set; }

    public string? Operation { get; set; }

    public string? UserId { get; set; }

    public string? ContextJson { get; set; }

    public string? DetailsJson { get; set; }
}
