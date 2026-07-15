using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class BrandStyleConfiguration : IEntityTypeConfiguration<BrandStyle>
{
    public void Configure(EntityTypeBuilder<BrandStyle> builder) =>
        builder.ConfigureSimpleLookup("BrandStyles", s => s.StyleName, 50);
}
