using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class CtaTemplateConfiguration : IEntityTypeConfiguration<CtaTemplate>
{
    public void Configure(EntityTypeBuilder<CtaTemplate> builder) =>
        builder.ConfigureSimpleLookup("CtaTemplates", c => c.CtaText, 100);
}
