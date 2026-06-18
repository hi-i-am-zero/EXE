using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.ConfigureBaseEntity();

        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.TransactionId).HasMaxLength(128);
        builder.Property(p => p.FailureReason).HasMaxLength(1000);

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.InvoiceId);
        builder.HasIndex(p => p.TransactionId).HasFilter("[TransactionId] IS NOT NULL");

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
