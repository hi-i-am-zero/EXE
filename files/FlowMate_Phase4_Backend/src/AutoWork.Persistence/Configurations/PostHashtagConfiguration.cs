using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PostHashtagConfiguration : IEntityTypeConfiguration<PostHashtag>
{
    public void Configure(EntityTypeBuilder<PostHashtag> builder)
    {
        builder.ToTable("PostHashtags");
        builder.ConfigureBaseEntity();

        builder.HasIndex(x => new { x.PostId, x.HashtagId }).IsUnique();

        builder.HasOne(x => x.Post)
            .WithMany(p => p.PostHashtags)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Hashtag)
            .WithMany(h => h.PostHashtags)
            .HasForeignKey(x => x.HashtagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
