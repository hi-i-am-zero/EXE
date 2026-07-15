using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class CampaignChannelAccountConfiguration : IEntityTypeConfiguration<CampaignChannelAccount>
{
    public void Configure(EntityTypeBuilder<CampaignChannelAccount> builder)
    {
        builder.ToTable("CampaignChannelAccounts");
        builder.ConfigureBaseEntity();

        builder.HasIndex(x => new { x.CampaignId, x.ChannelAccountId }).IsUnique();

        builder.HasOne(x => x.Campaign)
            .WithMany(c => c.CampaignChannelAccounts)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChannelAccount)
            .WithMany(ca => ca.CampaignChannelAccounts)
            .HasForeignKey(x => x.ChannelAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
