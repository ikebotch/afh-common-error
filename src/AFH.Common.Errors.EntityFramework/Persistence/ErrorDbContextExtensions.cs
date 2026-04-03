using AFH.Common.Errors.EntityFramework.Entities;
using AFH.Common.Errors.EntityFramework.Mappings;
using Microsoft.EntityFrameworkCore;

namespace AFH.Common.Errors.EntityFramework.Persistence;

public static class ErrorDbContextExtensions
{
    public static ModelBuilder AddErrorRecordEntity(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new ErrorRecordEntityConfiguration());
        return modelBuilder;
    }

    public static DbSet<ErrorRecordEntity> ErrorRecords(this DbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return dbContext.Set<ErrorRecordEntity>();
    }
}
