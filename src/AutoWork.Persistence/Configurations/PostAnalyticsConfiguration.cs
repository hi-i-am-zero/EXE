using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PostAnalyticsConfiguration : IEntityTypeConfiguration<PostAnalytics>
{
    public void Configure(EntityTypeBuilder<PostAnalytics> builder)
    {
        builder.ToTable("PostAnalytics");
        builder.ConfigureBaseEntity();
        builder.HasIndex(a => a.PostChannelAccountId).IsUnique();
    }
}
