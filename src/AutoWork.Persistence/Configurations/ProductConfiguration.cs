using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.ConfigureBaseEntity();
        builder.Property(p => p.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");

        builder.HasIndex(p => p.BrandProfileId);

        builder.HasOne(p => p.BrandProfile)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ProductCategory)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.ProductCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
