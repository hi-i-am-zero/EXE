using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class TimelineConfiguration : IEntityTypeConfiguration<Timeline>
{
    public void Configure(EntityTypeBuilder<Timeline> builder)
    {
        builder.ToTable("Timelines", t => t.HasCheckConstraint("CK_Timelines_Dates", "[EndDate] >= [StartDate]"));
        builder.ConfigureBaseEntity();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();

        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.CampaignId);

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Timelines)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Campaign)
            .WithMany(c => c.Timelines)
            .HasForeignKey(t => t.CampaignId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.TemplateType)
            .WithMany(tt => tt.Timelines)
            .HasForeignKey(t => t.TemplateTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
