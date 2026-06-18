using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PostLogConfiguration : IEntityTypeConfiguration<PostLog>
{
    public void Configure(EntityTypeBuilder<PostLog> builder)
    {
        builder.ToTable("PostLogs");
        builder.ConfigureBaseEntity();

        builder.Property(pl => pl.Action).HasMaxLength(100).IsRequired();
        builder.Property(pl => pl.Message).HasMaxLength(2000).IsRequired();

        builder.HasIndex(pl => pl.PostId);
        builder.HasIndex(pl => pl.CreatedAt);

        builder.HasOne(pl => pl.Post)
            .WithMany(p => p.Logs)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
