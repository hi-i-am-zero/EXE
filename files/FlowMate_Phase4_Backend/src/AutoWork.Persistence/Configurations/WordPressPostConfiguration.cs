using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class WordPressPostConfiguration : IEntityTypeConfiguration<WordPressPost>
{
    public void Configure(EntityTypeBuilder<WordPressPost> builder)
    {
        builder.ToTable("WordPressPosts");
        builder.ConfigureBaseEntity();

        builder.Property(wp => wp.ExternalPostId).HasMaxLength(128).IsRequired();
        builder.Property(wp => wp.Title).HasMaxLength(500).IsRequired();
        builder.Ignore(wp => wp.Excerpt);
        builder.Property(wp => wp.Permalink).HasMaxLength(1000).HasColumnName("PublishedUrl");

        builder.HasIndex(wp => wp.WordPressSiteId);
        builder.HasIndex(wp => wp.Status);

        builder.HasOne(wp => wp.WordPressSite)
            .WithMany(ws => ws.Posts)
            .HasForeignKey(wp => wp.WordPressSiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
