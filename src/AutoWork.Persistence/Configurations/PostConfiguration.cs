using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.ConfigureBaseEntity();

        builder.Property(p => p.Title).HasMaxLength(500).IsRequired();
        builder.Property(p => p.ExternalPostId).HasMaxLength(128);
        builder.Property(p => p.PublishedUrl).HasMaxLength(1000);

        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.ChannelAccountId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.PublishedAt);

        builder.HasOne(p => p.Project)
            .WithMany(pr => pr.Posts)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ChannelAccount)
            .WithMany(ca => ca.Posts)
            .HasForeignKey(p => p.ChannelAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
