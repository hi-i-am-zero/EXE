using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class TimelineTemplateTypeConfiguration : IEntityTypeConfiguration<TimelineTemplateType>
{
    public void Configure(EntityTypeBuilder<TimelineTemplateType> builder)
    {
        builder.ToTable("TimelineTemplateTypes");
        builder.ConfigureBaseEntity();
        builder.Property(t => t.TypeName).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(300);
        builder.HasIndex(t => t.TypeName).IsUnique();
    }
}
