using AFH.Common.Errors.EntityFramework.Entities;
using AFH.Common.Errors.EntityFramework.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AFH.Common.Errors.EntityFramework.Tests.Fixtures;

internal sealed class TestErrorDbContext(DbContextOptions<TestErrorDbContext> options) : DbContext(options)
{
    public DbSet<ErrorRecordEntity> ErrorRecords => Set<ErrorRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddErrorRecordEntity();
    }
}
