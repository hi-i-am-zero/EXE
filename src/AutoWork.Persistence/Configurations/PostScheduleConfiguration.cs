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
        // FlowMate v2 uses PostChannelAccountId; EF legacy uses PostId (compat column).
        builder.Property<Guid?>("PostChannelAccountId");

        builder.HasIndex(ps => ps.PostId);
        builder.HasIndex(ps => new { ps.Status, ps.ScheduledAt });

        builder.HasOne(ps => ps.Post)
            .WithOne(p => p.Schedule)
            .HasForeignKey<PostSchedule>(ps => ps.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
