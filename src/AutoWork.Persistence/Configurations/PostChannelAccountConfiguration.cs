using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PostChannelAccountConfiguration : IEntityTypeConfiguration<PostChannelAccount>
{
    public void Configure(EntityTypeBuilder<PostChannelAccount> builder)
    {
        builder.ToTable("PostChannelAccounts");
        builder.ConfigureBaseEntity();

        builder.Property(x => x.ExternalPostId).HasMaxLength(128);
        builder.Property(x => x.PublishedUrl).HasMaxLength(1000);

        // Không đăng trùng 1 kênh 2 lần cho cùng 1 bài viết
        builder.HasIndex(x => new { x.PostId, x.ChannelAccountId }).IsUnique();

        builder.HasOne(x => x.Post)
            .WithMany(p => p.PostChannelAccounts)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChannelAccount)
            .WithMany(ca => ca.PostChannelAccounts)
            .HasForeignKey(x => x.ChannelAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Analytics)
            .WithOne(a => a.PostChannelAccount)
            .HasForeignKey<PostAnalytics>(a => a.PostChannelAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
