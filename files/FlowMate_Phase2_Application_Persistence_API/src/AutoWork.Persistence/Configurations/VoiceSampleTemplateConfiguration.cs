using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class VoiceSampleTemplateConfiguration : IEntityTypeConfiguration<VoiceSampleTemplate>
{
    public void Configure(EntityTypeBuilder<VoiceSampleTemplate> builder)
    {
        builder.ToTable("VoiceSampleTemplates");
        builder.ConfigureBaseEntity();
        builder.Property(v => v.SampleText).HasMaxLength(500).IsRequired();
    }
}
