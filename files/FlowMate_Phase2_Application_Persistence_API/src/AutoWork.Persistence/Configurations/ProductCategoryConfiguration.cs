using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.ConfigureBaseEntity();
        builder.Property(c => c.CategoryName).HasMaxLength(100).IsRequired();

        builder.HasIndex(c => new { c.BrandProfileId, c.CategoryName }).IsUnique();

        builder.HasOne(c => c.BrandProfile)
            .WithMany(b => b.ProductCategories)
            .HasForeignKey(c => c.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
