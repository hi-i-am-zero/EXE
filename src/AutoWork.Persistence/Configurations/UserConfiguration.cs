using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.ConfigureBaseEntity();

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.GoogleId).HasMaxLength(128);
        builder.Property(u => u.FacebookId).HasMaxLength(128);
        builder.Property(u => u.ReferralCode).HasMaxLength(32);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.ReferralCode).IsUnique().HasFilter("[ReferralCode] IS NOT NULL");
        builder.HasIndex(u => u.GoogleId).HasFilter("[GoogleId] IS NOT NULL");
        builder.HasIndex(u => u.FacebookId).HasFilter("[FacebookId] IS NOT NULL");

        builder.HasOne(u => u.ReferredByUser)
            .WithMany(u => u.ReferredUsers)
            .HasForeignKey(u => u.ReferredByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
