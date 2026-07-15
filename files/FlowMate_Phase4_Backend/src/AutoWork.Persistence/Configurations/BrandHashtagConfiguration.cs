using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class BrandHashtagConfiguration : IEntityTypeConfiguration<BrandHashtag>
{
    public void Configure(EntityTypeBuilder<BrandHashtag> builder)
    {
        builder.ToTable("BrandHashtags");
        builder.ConfigureBaseEntity();

        builder.HasIndex(x => new { x.BrandProfileId, x.HashtagId }).IsUnique();

        builder.HasOne(x => x.BrandProfile)
            .WithMany(b => b.BrandHashtags)
            .HasForeignKey(x => x.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Hashtag)
            .WithMany(h => h.BrandHashtags)
            .HasForeignKey(x => x.HashtagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
