using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class AffiliateCommissionConfiguration : IEntityTypeConfiguration<AffiliateCommission>
{
    public void Configure(EntityTypeBuilder<AffiliateCommission> builder)
    {
        builder.ToTable("AffiliateCommissions");
        builder.ConfigureBaseEntity();

        builder.Property(ac => ac.Amount).HasPrecision(18, 2);

        builder.HasIndex(ac => ac.AffiliateId);
        builder.HasIndex(ac => ac.ReferredUserId);
        builder.HasIndex(ac => ac.Status);

        builder.HasOne(ac => ac.Affiliate)
            .WithMany(a => a.Commissions)
            .HasForeignKey(ac => ac.AffiliateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ac => ac.ReferredUser)
            .WithMany(u => u.AffiliateCommissions)
            .HasForeignKey(ac => ac.ReferredUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ac => ac.Subscription)
            .WithMany(s => s.AffiliateCommissions)
            .HasForeignKey(ac => ac.SubscriptionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
