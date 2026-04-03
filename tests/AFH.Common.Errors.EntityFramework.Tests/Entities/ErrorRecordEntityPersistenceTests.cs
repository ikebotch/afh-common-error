using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.EntityFramework.DependencyInjection;
using AFH.Common.Errors.EntityFramework.Entities;
using AFH.Common.Errors.EntityFramework.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AFH.Common.Errors.EntityFramework.Tests.Entities;

public sealed class ErrorRecordEntityPersistenceTests
{
    [Fact]
    public async Task WriteAsync_PersistsMappedEntityValues()
    {
        using var scope = CreateServiceProvider().CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IErrorPersistenceWriter>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestErrorDbContext>();

        await writer.WriteAsync(ErrorRecordFactory.Create());

        var entity = await dbContext.Set<ErrorRecordEntity>().SingleAsync();

        entity.Code.Should().Be("dependency.failure");
        entity.Category.Should().Be("Dependency");
        entity.Severity.Should().Be("Error");
        entity.Operation.Should().Be("PersistError");
        entity.CorrelationId.Should().Be("corr-1");
        entity.UserId.Should().Be("user-1");
        entity.ContextJson.Should().Contain("\"traceId\":\"trace-1\"");
        entity.ContextJson.Should().Contain("\"tenantId\":\"tenant-1\"");
        entity.DetailsJson.Should().Contain("\"code\":\"dependency.failure\"");
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestErrorDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddAfhCommonErrorsEntityFramework<TestErrorDbContext>();
        return services.BuildServiceProvider();
    }
}
