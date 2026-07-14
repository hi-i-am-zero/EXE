using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class BrandColorConfiguration : IEntityTypeConfiguration<BrandColor>
{
    public void Configure(EntityTypeBuilder<BrandColor> builder)
    {
        builder.ToTable("BrandColors");
        builder.ConfigureBaseEntity();
        builder.Property(c => c.ColorHex).HasMaxLength(9).IsRequired();
        builder.Property(c => c.ColorType).HasMaxLength(20).IsRequired().HasDefaultValue("Secondary");

        builder.HasOne(c => c.BrandProfile)
            .WithMany(b => b.Colors)
            .HasForeignKey(c => c.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
