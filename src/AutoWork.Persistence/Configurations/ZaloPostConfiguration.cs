using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class ZaloPostConfiguration : IEntityTypeConfiguration<ZaloPost>
{
    public void Configure(EntityTypeBuilder<ZaloPost> builder)
    {
        builder.ToTable("ZaloPosts");
        builder.ConfigureBaseEntity();

        builder.Property(zp => zp.ExternalPostId).HasMaxLength(128);
        builder.Property(zp => zp.Content).IsRequired();

        builder.HasIndex(zp => zp.ZaloAccountId);
        builder.HasIndex(zp => zp.Status);

        builder.HasOne(zp => zp.ZaloAccount)
            .WithMany(za => za.Posts)
            .HasForeignKey(zp => zp.ZaloAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
