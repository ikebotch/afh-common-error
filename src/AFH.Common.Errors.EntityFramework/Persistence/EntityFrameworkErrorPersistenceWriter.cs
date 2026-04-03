using System.Text.Json;
using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.EntityFramework.Entities;
using AFH.Common.Errors.Models;
using Microsoft.EntityFrameworkCore;

namespace AFH.Common.Errors.EntityFramework.Persistence;

public sealed class EntityFrameworkErrorPersistenceWriter<TContext> : IErrorPersistenceWriter
    where TContext : DbContext
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly TContext _dbContext;

    public EntityFrameworkErrorPersistenceWriter(TContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task WriteAsync(ErrorRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        var entity = Map(record);
        await _dbContext.Set<ErrorRecordEntity>().AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    internal static ErrorRecordEntity Map(ErrorRecord record)
    {
        return new ErrorRecordEntity
        {
            Id = Guid.NewGuid(),
            Code = record.Code,
            Category = record.Category.ToString(),
            Severity = record.Severity.ToString(),
            Message = record.Message,
            ExceptionType = record.ExceptionType,
            StackTrace = record.StackTrace,
            OccurredUtc = record.OccurredUtc,
            TraceId = record.Context?.TraceId,
            CorrelationId = record.Context?.CorrelationId,
            Path = record.Context?.Path,
            Method = record.Context?.Method,
            Operation = record.Context?.Operation,
            UserId = record.Context?.UserId,
            ContextJson = record.Context is null ? null : JsonSerializer.Serialize(record.Context, SerializerOptions),
            DetailsJson = record.Details.Count == 0 ? null : JsonSerializer.Serialize(record.Details, SerializerOptions)
        };
    }
}
