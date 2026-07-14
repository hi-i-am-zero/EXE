using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PromotionTypeConfiguration : IEntityTypeConfiguration<PromotionType>
{
    public void Configure(EntityTypeBuilder<PromotionType> builder) =>
        builder.ConfigureSimpleLookup("PromotionTypes", t => t.TypeName, 100);
}
