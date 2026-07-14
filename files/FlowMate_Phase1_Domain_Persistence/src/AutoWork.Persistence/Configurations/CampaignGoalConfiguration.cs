using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class CampaignGoalConfiguration : IEntityTypeConfiguration<CampaignGoal>
{
    public void Configure(EntityTypeBuilder<CampaignGoal> builder) =>
        builder.ConfigureSimpleLookup("CampaignGoals", g => g.GoalName, 100);
}
