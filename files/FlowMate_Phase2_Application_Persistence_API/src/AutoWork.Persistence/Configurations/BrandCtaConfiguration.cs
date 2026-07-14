using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class BrandCtaConfiguration : IEntityTypeConfiguration<BrandCta>
{
    public void Configure(EntityTypeBuilder<BrandCta> builder)
    {
        builder.ToTable("BrandCtas");
        builder.ConfigureBaseEntity();

        builder.HasIndex(x => new { x.BrandProfileId, x.CtaId }).IsUnique();

        builder.HasOne(x => x.BrandProfile)
            .WithMany(b => b.BrandCtas)
            .HasForeignKey(x => x.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Cta)
            .WithMany(c => c.BrandCtas)
            .HasForeignKey(x => x.CtaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
