using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class WordPressSiteConfiguration : IEntityTypeConfiguration<WordPressSite>
{
    public void Configure(EntityTypeBuilder<WordPressSite> builder)
    {
        builder.ToTable("WordPressSites");
        builder.ConfigureBaseEntity();

        builder.Property(ws => ws.SiteUrl).HasMaxLength(500).IsRequired();
        builder.Property(ws => ws.SiteName).HasMaxLength(200).IsRequired();
        builder.Property(ws => ws.Username).HasMaxLength(100).IsRequired();

        builder.HasIndex(ws => ws.UserId);

        builder.HasOne(ws => ws.User)
            .WithMany(u => u.WordPressSites)
            .HasForeignKey(ws => ws.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
