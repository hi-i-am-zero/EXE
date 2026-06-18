using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        builder.ConfigureBaseEntity();

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.Name).IsUnique();
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.SortOrder);
    }
}
