using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PostContentConfiguration : IEntityTypeConfiguration<PostContent>
{
    public void Configure(EntityTypeBuilder<PostContent> builder)
    {
        builder.ToTable("PostContents");
        builder.ConfigureBaseEntity();

        builder.Property(pc => pc.Content).IsRequired();

        builder.HasIndex(pc => new { pc.PostId, pc.SortOrder });

        builder.HasOne(pc => pc.Post)
            .WithMany(p => p.Contents)
            .HasForeignKey(pc => pc.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.MediaFile)
            .WithMany(m => m.PostContents)
            .HasForeignKey(pc => pc.MediaFileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
