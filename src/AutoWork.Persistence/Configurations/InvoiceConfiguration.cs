using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.ConfigureBaseEntity();

        builder.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.Property(i => i.Tax).HasPrecision(18, 2);
        builder.Property(i => i.Total).HasPrecision(18, 2);
        builder.Property(i => i.Notes).HasMaxLength(1000);

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => i.UserId);
        builder.HasIndex(i => i.Status);

        builder.HasOne(i => i.User)
            .WithMany(u => u.Invoices)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Subscription)
            .WithMany(s => s.Invoices)
            .HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
