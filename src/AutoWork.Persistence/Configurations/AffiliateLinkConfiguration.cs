using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class AffiliateLinkConfiguration : IEntityTypeConfiguration<AffiliateLink>
{
    public void Configure(EntityTypeBuilder<AffiliateLink> builder)
    {
        builder.ToTable("AffiliateLinks");
        builder.ConfigureBaseEntity();

        builder.Property(al => al.Url).HasMaxLength(1000).IsRequired();
        builder.Property(al => al.Campaign).HasMaxLength(100);

        builder.HasIndex(al => al.AffiliateId);

        builder.HasOne(al => al.Affiliate)
            .WithMany(a => a.Links)
            .HasForeignKey(al => al.AffiliateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
