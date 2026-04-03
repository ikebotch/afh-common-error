using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.EntityFramework.DependencyInjection;
using AFH.Common.Errors.EntityFramework.Entities;
using AFH.Common.Errors.EntityFramework.Persistence;
using AFH.Common.Errors.EntityFramework.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AFH.Common.Errors.EntityFramework.Tests.Persistence;

public sealed class ErrorDbContextExtensionsTests
{
    [Fact]
    public void ErrorRecords_ReturnsEntitySet()
    {
        using DbContext dbContext = CreateDbContext();

        dbContext.ErrorRecords().Should().BeSameAs(dbContext.Set<ErrorRecordEntity>());
    }

    [Fact]
    public void AddAfhCommonErrorsEntityFramework_RegistersPersistenceWriter()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestErrorDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddAfhCommonErrorsEntityFramework<TestErrorDbContext>();

        using var provider = services.BuildServiceProvider();

        provider.GetService<IErrorPersistenceWriter>().Should().NotBeNull();
    }

    private static TestErrorDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestErrorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestErrorDbContext(options);
    }
}
