using AFH.Common.Errors.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AFH.Common.Errors.EntityFramework.Mappings;

public sealed class ErrorRecordEntityConfiguration : IEntityTypeConfiguration<ErrorRecordEntity>
{
    public void Configure(EntityTypeBuilder<ErrorRecordEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Code)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(entity => entity.Category)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(entity => entity.Severity)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(entity => entity.Message)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(entity => entity.ExceptionType)
            .HasMaxLength(512);

        builder.Property(entity => entity.TraceId)
            .HasMaxLength(256);

        builder.Property(entity => entity.CorrelationId)
            .HasMaxLength(256);

        builder.Property(entity => entity.Path)
            .HasMaxLength(512);

        builder.Property(entity => entity.Method)
            .HasMaxLength(32);

        builder.Property(entity => entity.Operation)
            .HasMaxLength(256);

        builder.Property(entity => entity.UserId)
            .HasMaxLength(256);

        builder.Property(entity => entity.ContextJson);
        builder.Property(entity => entity.DetailsJson);
        builder.Property(entity => entity.StackTrace);
        builder.Property(entity => entity.OccurredUtc).IsRequired();

        builder.HasIndex(entity => entity.Code);
        builder.HasIndex(entity => entity.OccurredUtc);
    }
}
