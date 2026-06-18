using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class ZaloAccountConfiguration : IEntityTypeConfiguration<ZaloAccount>
{
    public void Configure(EntityTypeBuilder<ZaloAccount> builder)
    {
        builder.ToTable("ZaloAccounts");
        builder.ConfigureBaseEntity();

        builder.Property(za => za.ZaloUserId).HasMaxLength(128).IsRequired();
        builder.Property(za => za.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(za => za.AvatarUrl).HasMaxLength(500);

        builder.HasIndex(za => za.UserId);
        builder.HasIndex(za => za.ZaloUserId);

        builder.HasOne(za => za.User)
            .WithMany(u => u.ZaloAccounts)
            .HasForeignKey(za => za.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
