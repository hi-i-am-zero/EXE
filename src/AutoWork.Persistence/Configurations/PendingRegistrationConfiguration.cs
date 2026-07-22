using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PendingRegistrationConfiguration : IEntityTypeConfiguration<PendingRegistration>
{
    public void Configure(EntityTypeBuilder<PendingRegistration> builder)
    {
        builder.ToTable("PendingRegistrations");

        builder.Property(p => p.Email).HasMaxLength(256).IsRequired();
        builder.Property(p => p.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Phone).HasMaxLength(20).IsRequired();
        builder.Property(p => p.ReferralCode).HasMaxLength(32).IsRequired();
        builder.Property(p => p.Token).HasMaxLength(128).IsRequired();

        builder.HasIndex(p => p.Token).IsUnique();
        builder.HasIndex(p => p.Email);
        builder.HasIndex(p => p.Phone);
    }
}
