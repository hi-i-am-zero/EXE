using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Common;

namespace AutoWork.Application.Common.Helpers;

/// <summary>
/// Đồng bộ 1 tập lựa chọn N-N (VD: BrandProfile chọn nhiều BrandStyle) theo kiểu diff:
/// thêm dòng còn thiếu, xoá dòng không còn được chọn — thay vì xoá hết rồi tạo lại
/// (giữ nguyên CreatedAt/Id của các lựa chọn không đổi).
///
/// Dùng chung cho BrandProfileStyles, BrandProfileKeywords, BrandCtas, BrandHashtags —
/// 4 bảng join có cùng hình dạng (chỉ khác entity join và cột lookup id).
/// </summary>
public static class ManyToManySync
{
    public static async Task ReplaceSelectionAsync<TJoin>(
        IRepository<TJoin> repository,
        IReadOnlyList<TJoin> currentRows,
        IEnumerable<Guid> desiredLookupIds,
        Func<Guid, TJoin> createRow,
        Func<TJoin, Guid> getLookupId,
        CancellationToken cancellationToken = default)
        where TJoin : BaseEntity
    {
        var desired = desiredLookupIds.Distinct().ToHashSet();
        var currentLookupIds = currentRows.Select(getLookupId).ToHashSet();

        var toRemove = currentRows.Where(r => !desired.Contains(getLookupId(r))).ToList();
        var toAddIds = desired.Where(id => !currentLookupIds.Contains(id)).ToList();

        foreach (var row in toRemove)
        {
            await repository.DeleteAsync(row, cancellationToken);
        }

        foreach (var lookupId in toAddIds)
        {
            await repository.AddAsync(createRow(lookupId), cancellationToken);
        }
    }
}
