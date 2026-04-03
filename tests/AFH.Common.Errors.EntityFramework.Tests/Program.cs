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
            MapsErrorRecordIntoEntity,
            AppliesEntityConfigurationToModel,
            RegistersPersistenceWriterInDependencyInjection,
            WritesErrorRecordToDbContext
        };

        foreach (var test in tests)
        {
            test();
        }

        Console.WriteLine($"Executed {tests.Length} entity framework adapter tests successfully.");
        return 0;
    }

    private static void MapsErrorRecordIntoEntity()
    {
        var record = CreateRecord();
        var entity = EntityFrameworkErrorPersistenceWriter<TestErrorDbContext>.Map(record);

        Assert.Equal("dependency.failure", entity.Code);
        Assert.Equal("Dependency", entity.Category);
        Assert.Equal("Error", entity.Severity);
        Assert.Equal("trace-1", entity.TraceId);
    }

    private static void AppliesEntityConfigurationToModel()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        modelBuilder.AddErrorRecordEntity();

        var entityType = modelBuilder.Model.FindEntityType(typeof(ErrorRecordEntity));

        Assert.NotNull(entityType);
        Assert.Equal("ErrorRecords", entityType!.GetTableName());
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
        var services = new ServiceCollection();
        services.AddDbContext<TestErrorDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddAfhCommonErrorsEntityFramework<TestErrorDbContext>();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IErrorPersistenceWriter>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestErrorDbContext>();

        writer.WriteAsync(CreateRecord()).GetAwaiter().GetResult();

        Assert.Equal(1, dbContext.Set<ErrorRecordEntity>().Count());
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

    public static void NotNull(object? value)
    {
        if (value is null)
        {
            throw new InvalidOperationException("Expected value to be non-null.");
        }
    }
}
