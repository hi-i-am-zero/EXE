using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

/// <summary>Business rule "tối đa 2 phong cách" được enforce ở Application layer (FluentValidation),
/// không ở DB, vì SQL Server CHECK constraint không diễn đạt được COUNT theo nhóm.</summary>
public class BrandProfileStyleConfiguration : IEntityTypeConfiguration<BrandProfileStyle>
{
    public void Configure(EntityTypeBuilder<BrandProfileStyle> builder)
    {
        builder.ToTable("BrandProfileStyles");
        builder.ConfigureBaseEntity();

        builder.HasIndex(x => new { x.BrandProfileId, x.StyleId }).IsUnique();

        builder.HasOne(x => x.BrandProfile)
            .WithMany(b => b.BrandProfileStyles)
            .HasForeignKey(x => x.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Style)
            .WithMany(s => s.BrandProfileStyles)
            .HasForeignKey(x => x.StyleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
