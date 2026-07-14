using System.Linq.Expressions;
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

    /// <summary>
    /// Cấu hình dùng chung cho các bảng lookup đơn giản chỉ có 1 cột tên duy nhất
    /// (VD: BrandStyle.StyleName, Hashtag.Tag, CampaignGoal.GoalName...).
    /// Gom lại để tránh lặp ToTable + ConfigureBaseEntity + MaxLength + Unique Index
    /// ở từng Configuration riêng lẻ.
    /// </summary>
    public static void ConfigureSimpleLookup<T>(
        this EntityTypeBuilder<T> builder,
        string tableName,
        Expression<Func<T, string>> nameSelector,
        int maxLength) where T : BaseEntity
    {
        builder.ToTable(tableName);
        builder.ConfigureBaseEntity();
        builder.Property(nameSelector).HasMaxLength(maxLength).IsRequired();
        builder.HasIndex(nameSelector).IsUnique();
    }
}
