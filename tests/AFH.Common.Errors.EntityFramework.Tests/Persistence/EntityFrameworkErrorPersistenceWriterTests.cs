using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.EntityFramework.DependencyInjection;
using AFH.Common.Errors.EntityFramework.Entities;
using AFH.Common.Errors.EntityFramework.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AFH.Common.Errors.EntityFramework.Tests.Persistence;

public sealed class EntityFrameworkErrorPersistenceWriterTests
{
    [Fact]
    public async Task WriteAsync_WhenContextAndDetailsAreMissing_LeavesJsonColumnsNull()
    {
        using var scope = CreateServiceProvider().CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IErrorPersistenceWriter>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestErrorDbContext>();

        await writer.WriteAsync(ErrorRecordFactory.CreateWithoutContextOrDetails());

        var entity = await dbContext.Set<ErrorRecordEntity>().SingleAsync();

        entity.ContextJson.Should().BeNull();
        entity.DetailsJson.Should().BeNull();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestErrorDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddAfhCommonErrorsEntityFramework<TestErrorDbContext>();
        return services.BuildServiceProvider();
    }
}
