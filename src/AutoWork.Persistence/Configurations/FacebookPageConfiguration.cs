using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class FacebookPageConfiguration : IEntityTypeConfiguration<FacebookPage>
{
    public void Configure(EntityTypeBuilder<FacebookPage> builder)
    {
        builder.ToTable("FacebookPages");
        builder.ConfigureBaseEntity();

        builder.Property(fp => fp.PageId).HasMaxLength(128).IsRequired();
        builder.Property(fp => fp.Name).HasMaxLength(200).IsRequired();
        builder.Property(fp => fp.Category).HasMaxLength(100);
        builder.Property(fp => fp.ProfilePictureUrl).HasMaxLength(500);

        builder.HasIndex(fp => fp.FacebookAccountId);
        builder.HasIndex(fp => fp.PageId);

        builder.HasOne(fp => fp.FacebookAccount)
            .WithMany(fa => fa.Pages)
            .HasForeignKey(fp => fp.FacebookAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
