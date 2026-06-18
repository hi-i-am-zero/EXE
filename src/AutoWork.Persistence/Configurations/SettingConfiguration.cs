using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");
        builder.ConfigureBaseEntity();

        builder.Property(s => s.Key).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Value).IsRequired();
        builder.Property(s => s.Category).HasMaxLength(100);
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasIndex(s => s.Key).IsUnique();
        builder.HasIndex(s => s.Category);
    }
}
