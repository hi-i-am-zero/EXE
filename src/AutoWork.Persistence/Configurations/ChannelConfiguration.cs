using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("Channels");
        builder.ConfigureBaseEntity();

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.IconUrl).HasMaxLength(500);

        builder.HasIndex(c => c.Code).IsUnique();
    }
}
