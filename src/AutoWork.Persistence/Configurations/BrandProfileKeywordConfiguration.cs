using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

/// <summary>Khuyến nghị "3-5 từ khóa" được enforce ở Application layer (FluentValidation), không ở DB.</summary>
public class BrandProfileKeywordConfiguration : IEntityTypeConfiguration<BrandProfileKeyword>
{
    public void Configure(EntityTypeBuilder<BrandProfileKeyword> builder)
    {
        builder.ToTable("BrandProfileKeywords");
        builder.ConfigureBaseEntity();

        builder.HasIndex(x => new { x.BrandProfileId, x.ToneKeywordId }).IsUnique();

        builder.HasOne(x => x.BrandProfile)
            .WithMany(b => b.BrandProfileKeywords)
            .HasForeignKey(x => x.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ToneKeyword)
            .WithMany(k => k.BrandProfileKeywords)
            .HasForeignKey(x => x.ToneKeywordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
