using AFH.Common.Errors.EntityFramework.Entities;
using AFH.Common.Errors.EntityFramework.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

namespace AFH.Common.Errors.EntityFramework.Tests.Mappings;

public sealed class ErrorRecordEntityConfigurationTests
{
    [Fact]
    public void AddErrorRecordEntity_AppliesExpectedModelConfiguration()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        modelBuilder.AddErrorRecordEntity();

        var entityType = modelBuilder.Model.FindEntityType(typeof(ErrorRecordEntity));
        var codeProperty = entityType?.FindProperty(nameof(ErrorRecordEntity.Code));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be(nameof(ErrorRecordEntity));
        codeProperty.Should().NotBeNull();
        codeProperty!.GetMaxLength().Should().Be(128);
        entityType!.GetIndexes().Should().HaveCount(2);
    }
}
