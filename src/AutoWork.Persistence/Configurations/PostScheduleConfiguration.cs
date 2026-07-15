using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PostScheduleConfiguration : IEntityTypeConfiguration<PostSchedule>
{
    public void Configure(EntityTypeBuilder<PostSchedule> builder)
    {
        builder.ToTable("PostSchedules");
        builder.ConfigureBaseEntity();

        builder.Property(ps => ps.FailureReason).HasMaxLength(1000);

        builder.HasIndex(ps => ps.PostId);
        builder.HasIndex(ps => ps.PostChannelAccountId);
        builder.HasIndex(ps => new { ps.Status, ps.ScheduledAt });

        builder.HasOne(ps => ps.Post)
            .WithOne(p => p.Schedule)
            .HasForeignKey<PostSchedule>(ps => ps.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // FlowMate v2: cho phép retry/lịch riêng theo từng ChannelAccount của 1 bài viết
        builder.HasOne(ps => ps.PostChannelAccount)
            .WithMany(pca => pca.Schedules)
            .HasForeignKey(ps => ps.PostChannelAccountId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
