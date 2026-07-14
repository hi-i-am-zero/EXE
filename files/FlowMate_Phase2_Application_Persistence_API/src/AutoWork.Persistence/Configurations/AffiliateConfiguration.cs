using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class AffiliateConfiguration : IEntityTypeConfiguration<Affiliate>
{
    public void Configure(EntityTypeBuilder<Affiliate> builder)
    {
        builder.ToTable("Affiliates");
        builder.ConfigureBaseEntity();

        builder.Property(a => a.Code).HasMaxLength(32).IsRequired();
        builder.Property(a => a.CommissionRate).HasPrecision(5, 4);

        builder.HasIndex(a => a.UserId).IsUnique();
        builder.HasIndex(a => a.Code).IsUnique();

        builder.HasOne(a => a.User)
            .WithOne(u => u.Affiliate)
            .HasForeignKey<Affiliate>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
