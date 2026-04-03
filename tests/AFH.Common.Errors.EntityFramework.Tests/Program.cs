using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.Codes;
using AFH.Common.Errors.EntityFramework.DependencyInjection;
using AFH.Common.Errors.EntityFramework.Entities;
using AFH.Common.Errors.EntityFramework.Mappings;
using AFH.Common.Errors.EntityFramework.Persistence;
using AFH.Common.Errors.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

return TestRunner.Run();

internal static class TestRunner
{
    public static int Run()
    {
        var tests = new Action[]
        {
            AppliesEntityConfigurationToModel,
            ExposesErrorRecordsSetThroughExtension,
            RegistersPersistenceWriterInDependencyInjection,
            WritesErrorRecordToDbContext,
            PersistsMappedContextAndDetails,
            LeavesOptionalJsonColumnsNullWhenContextAndDetailsAreEmpty
        };

        foreach (var test in tests)
        {
            test();
        }

        Console.WriteLine($"Executed {tests.Length} entity framework adapter tests successfully.");
        return 0;
    }

    private static void AppliesEntityConfigurationToModel()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        modelBuilder.AddErrorRecordEntity();

        var entityType = modelBuilder.Model.FindEntityType(typeof(ErrorRecordEntity));
        var codeProperty = entityType?.FindProperty(nameof(ErrorRecordEntity.Code));

        Assert.NotNull(entityType);
        Assert.Equal("ErrorRecords", entityType!.GetTableName());
        Assert.Equal(128, codeProperty?.GetMaxLength());
        Assert.Equal(2, entityType.GetIndexes().Count());
    }

    private static void ExposesErrorRecordsSetThroughExtension()
    {
        using DbContext dbContext = CreateDbContext();

        Assert.Same(dbContext.Set<ErrorRecordEntity>(), dbContext.ErrorRecords());
    }

    private static void RegistersPersistenceWriterInDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestErrorDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddAfhCommonErrorsEntityFramework<TestErrorDbContext>();
        var provider = services.BuildServiceProvider();

        var writer = provider.GetService<IErrorPersistenceWriter>();

        Assert.NotNull(writer);
    }

    private static void WritesErrorRecordToDbContext()
    {
        using var scope = CreateServiceProvider().CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IErrorPersistenceWriter>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestErrorDbContext>();

        writer.WriteAsync(CreateRecord()).GetAwaiter().GetResult();

        var entity = dbContext.Set<ErrorRecordEntity>().Single();
        Assert.Equal(1, dbContext.Set<ErrorRecordEntity>().Count());
        Assert.Equal("dependency.failure", entity.Code);
        Assert.Equal("Dependency", entity.Category);
        Assert.Equal("Error", entity.Severity);
        Assert.Equal("PersistError", entity.Operation);
        Assert.Equal("corr-1", entity.CorrelationId);
    }

    private static void PersistsMappedContextAndDetails()
    {
        using var scope = CreateServiceProvider().CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IErrorPersistenceWriter>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestErrorDbContext>();

        writer.WriteAsync(CreateRecord()).GetAwaiter().GetResult();

        var entity = dbContext.Set<ErrorRecordEntity>().Single();
        Assert.Equal("trace-1", entity.TraceId);
        Assert.Contains("\"traceId\":\"trace-1\"", entity.ContextJson!);
        Assert.Contains("\"code\":\"dependency.failure\"", entity.DetailsJson!);
    }

    private static void LeavesOptionalJsonColumnsNullWhenContextAndDetailsAreEmpty()
    {
        using var scope = CreateServiceProvider().CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IErrorPersistenceWriter>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestErrorDbContext>();

        writer.WriteAsync(new ErrorRecord
        {
            Code = CommonErrorCodes.Unexpected.Value,
            Category = ErrorCategory.Unknown,
            Severity = ErrorSeverity.Critical,
            Message = "Unexpected."
        }).GetAwaiter().GetResult();

        var entity = dbContext.Set<ErrorRecordEntity>().Single();
        Assert.Null(entity.ContextJson);
        Assert.Null(entity.DetailsJson);
    }

    private static TestErrorDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestErrorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestErrorDbContext(options);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestErrorDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddAfhCommonErrorsEntityFramework<TestErrorDbContext>();
        return services.BuildServiceProvider();
    }

    private static ErrorRecord CreateRecord()
    {
        return new ErrorRecord
        {
            Code = DependencyErrorCodes.Failure.Value,
            Category = ErrorCategory.Dependency,
            Severity = ErrorSeverity.Error,
            Message = "Dependency failed.",
            ExceptionType = typeof(TimeoutException).FullName,
            OccurredUtc = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero),
            Context = new ErrorContext(
                TraceId: "trace-1",
                CorrelationId: "corr-1",
                Path: "/errors",
                Method: "POST",
                Operation: "PersistError"),
            Details =
            [
                new ErrorDetail(DependencyErrorCodes.Failure.Value, "Dependency failed.")
            ]
        };
    }
}

internal sealed class TestErrorDbContext : DbContext
{
    public TestErrorDbContext(DbContextOptions<TestErrorDbContext> options)
        : base(options)
    {
    }

    public DbSet<ErrorRecordEntity> ErrorRecords => Set<ErrorRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddErrorRecordEntity();
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

    public static void Null(object? value)
    {
        if (value is not null)
        {
            throw new InvalidOperationException("Expected value to be null.");
        }
    }

    public static void Same(object expected, object actual)
    {
        if (!ReferenceEquals(expected, actual))
        {
            throw new InvalidOperationException("Expected both values to reference the same instance.");
        }
    }
}
