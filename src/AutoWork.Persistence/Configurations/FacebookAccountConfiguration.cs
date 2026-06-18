using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class FacebookAccountConfiguration : IEntityTypeConfiguration<FacebookAccount>
{
    public void Configure(EntityTypeBuilder<FacebookAccount> builder)
    {
        builder.ToTable("FacebookAccounts");
        builder.ConfigureBaseEntity();

        builder.Property(fa => fa.FacebookUserId).HasMaxLength(128).IsRequired();
        builder.Property(fa => fa.Name).HasMaxLength(200).IsRequired();
        builder.Property(fa => fa.Email).HasMaxLength(256);
        builder.Property(fa => fa.ProfilePictureUrl).HasMaxLength(500);

        builder.HasIndex(fa => fa.UserId);
        builder.HasIndex(fa => fa.FacebookUserId);

        builder.HasOne(fa => fa.User)
            .WithMany(u => u.FacebookAccounts)
            .HasForeignKey(fa => fa.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
