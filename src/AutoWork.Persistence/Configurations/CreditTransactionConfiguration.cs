using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class CreditTransactionConfiguration : IEntityTypeConfiguration<CreditTransaction>
{
    public void Configure(EntityTypeBuilder<CreditTransaction> builder)
    {
        builder.ToTable("CreditTransactions");
        builder.ConfigureBaseEntity();

        builder.Property(ct => ct.Description).HasMaxLength(500);
        builder.Property(ct => ct.ReferenceType).HasMaxLength(100);

        builder.HasIndex(ct => ct.UserId);
        builder.HasIndex(ct => ct.CreditId);
        builder.HasIndex(ct => ct.CreatedAt);

        builder.HasOne(ct => ct.Credit)
            .WithMany(c => c.Transactions)
            .HasForeignKey(ct => ct.CreditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ct => ct.User)
            .WithMany(u => u.CreditTransactions)
            .HasForeignKey(ct => ct.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
