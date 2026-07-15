using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class AiPromptConfiguration : IEntityTypeConfiguration<AiPrompt>
{
    public void Configure(EntityTypeBuilder<AiPrompt> builder)
    {
        builder.ToTable("AiPrompts");
        builder.ConfigureBaseEntity();

        builder.Property(ap => ap.Name).HasMaxLength(200).IsRequired();
        builder.Property(ap => ap.Template).IsRequired();
        builder.Property(ap => ap.Category).HasMaxLength(100);

        builder.HasIndex(ap => ap.UserId);
        builder.HasIndex(ap => ap.IsSystem);

        builder.HasOne(ap => ap.User)
            .WithMany(u => u.AiPrompts)
            .HasForeignKey(ap => ap.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ap => ap.Project)
            .WithMany(p => p.AiPrompts)
            .HasForeignKey(ap => ap.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
