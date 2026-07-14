using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class BrandFontConfiguration : IEntityTypeConfiguration<BrandFont>
{
    public void Configure(EntityTypeBuilder<BrandFont> builder)
    {
        builder.ToTable("BrandFonts");
        builder.ConfigureBaseEntity();
        builder.Property(f => f.FontName).HasMaxLength(100).IsRequired();
        builder.Property(f => f.UsageType).HasMaxLength(20).IsRequired();

        builder.HasOne(f => f.BrandProfile)
            .WithMany(b => b.Fonts)
            .HasForeignKey(f => f.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
