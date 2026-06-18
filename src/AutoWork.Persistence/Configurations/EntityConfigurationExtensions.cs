using AutoWork.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

internal static class EntityConfigurationExtensions
{
    public static void ConfigureBaseEntity<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(e => e.IsDeleted);
    }
}
