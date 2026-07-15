using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns", t =>
        {
            t.HasCheckConstraint("CK_Campaigns_Dates", "[EndDate] >= [StartDate]");
            t.HasCheckConstraint("CK_Campaigns_NumberOfPosts", "[NumberOfPosts] > 0");
        });
        builder.ConfigureBaseEntity();

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.DiscountPercent).HasColumnType("decimal(5,2)");
        builder.Property(c => c.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(c => c.MinOrderAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(c => c.ProjectId);

        builder.HasOne(c => c.Project)
            .WithMany(p => p.Campaigns)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Goal)
            .WithMany(g => g.Campaigns)
            .HasForeignKey(c => c.GoalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PromotionType)
            .WithMany(t => t.Campaigns)
            .HasForeignKey(c => c.PromotionTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
