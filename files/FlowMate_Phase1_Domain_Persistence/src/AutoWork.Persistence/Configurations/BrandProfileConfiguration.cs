using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class BrandProfileConfiguration : IEntityTypeConfiguration<BrandProfile>
{
    public void Configure(EntityTypeBuilder<BrandProfile> builder)
    {
        builder.ToTable("BrandProfiles");
        builder.ConfigureBaseEntity();

        builder.Property(b => b.BusinessName).HasMaxLength(150).IsRequired();
        builder.Property(b => b.BrandName).HasMaxLength(150).IsRequired();
        builder.Property(b => b.Industry).HasMaxLength(100);
        builder.Property(b => b.ShortDescription).HasMaxLength(1000);
        builder.Property(b => b.Address).HasMaxLength(300);
        builder.Property(b => b.ContactPhone).HasMaxLength(20);
        builder.Property(b => b.ContactEmail).HasMaxLength(255);
        builder.Property(b => b.Website).HasMaxLength(255);
        builder.Property(b => b.LogoUrl).HasMaxLength(500);

        // Quan hệ 1-1 với Project — 1 workspace/shop chỉ có đúng 1 Brand Memory
        builder.HasIndex(b => b.ProjectId).IsUnique();

        builder.HasOne(b => b.Project)
            .WithOne(p => p.BrandProfile)
            .HasForeignKey<BrandProfile>(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.VoiceSample)
            .WithMany(v => v.BrandProfiles)
            .HasForeignKey(b => b.VoiceSampleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
