using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");
        builder.ConfigureBaseEntity();

        builder.Property(m => m.FileName).HasMaxLength(255).IsRequired();
        builder.Property(m => m.FileUrl).HasMaxLength(1000).IsRequired();
        builder.Property(m => m.MimeType).HasMaxLength(100).IsRequired();
        builder.Property(m => m.ThumbnailUrl).HasMaxLength(1000);

        builder.HasIndex(m => m.UserId);
        builder.HasIndex(m => m.ProjectId);
        builder.HasIndex(m => m.FileUrl);

        builder.HasOne(m => m.User)
            .WithMany(u => u.MediaFiles)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Project)
            .WithMany(p => p.MediaFiles)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
