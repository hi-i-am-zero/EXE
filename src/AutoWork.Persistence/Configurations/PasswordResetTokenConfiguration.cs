using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.ConfigureBaseEntity();

        builder.Property(prt => prt.Token).HasMaxLength(512).IsRequired();

        builder.HasIndex(prt => prt.Token).IsUnique();
        builder.HasIndex(prt => prt.UserId);

        builder.HasOne(prt => prt.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(prt => prt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
