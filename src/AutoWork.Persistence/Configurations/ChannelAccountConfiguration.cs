using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class ChannelAccountConfiguration : IEntityTypeConfiguration<ChannelAccount>
{
    public void Configure(EntityTypeBuilder<ChannelAccount> builder)
    {
        builder.ToTable("ChannelAccounts");
        builder.ConfigureBaseEntity();

        builder.Property(ca => ca.Name).HasMaxLength(200).IsRequired();
        builder.Property(ca => ca.ExternalId).HasMaxLength(128);
        builder.Property(ca => ca.ProfileUrl).HasMaxLength(500);
        builder.Property(ca => ca.AvatarUrl).HasMaxLength(500);

        builder.HasIndex(ca => new { ca.ProjectId, ca.ChannelId, ca.ExternalId });

        builder.HasOne(ca => ca.Project)
            .WithMany(p => p.ChannelAccounts)
            .HasForeignKey(ca => ca.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.Channel)
            .WithMany(c => c.ChannelAccounts)
            .HasForeignKey(ca => ca.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ca => ca.User)
            .WithMany(u => u.ChannelAccounts)
            .HasForeignKey(ca => ca.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
