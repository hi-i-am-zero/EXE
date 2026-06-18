using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class AiGeneratedContentConfiguration : IEntityTypeConfiguration<AiGeneratedContent>
{
    public void Configure(EntityTypeBuilder<AiGeneratedContent> builder)
    {
        builder.ToTable("AiGeneratedContents");
        builder.ConfigureBaseEntity();

        builder.Property(agc => agc.Input).IsRequired();
        builder.Property(agc => agc.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(agc => agc.UserId);
        builder.HasIndex(agc => agc.AiPromptId);
        builder.HasIndex(agc => agc.CreatedAt);

        builder.HasOne(agc => agc.AiPrompt)
            .WithMany(ap => ap.GeneratedContents)
            .HasForeignKey(agc => agc.AiPromptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(agc => agc.User)
            .WithMany(u => u.AiGeneratedContents)
            .HasForeignKey(agc => agc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(agc => agc.Project)
            .WithMany(p => p.AiGeneratedContents)
            .HasForeignKey(agc => agc.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
