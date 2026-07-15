using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Repositories;

public interface IBrandProfileRepository : IRepository<BrandProfile>
{
    /// <summary>Lấy đầy đủ Brand Memory (kèm styles/keywords/colors/fonts/CTA/hashtag/danh mục sản phẩm)
    /// theo ProjectId — dùng cho cả màn hiển thị lẫn trả về sau khi ghi.</summary>
    Task<BrandProfile?> GetByProjectIdWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>Danh sách ChannelAccount (kèm Channel) của 1 project — dùng để hiển thị mục
    /// "Kênh bán hàng" trong Brand Memory. Đặt ở đây thay vì tạo hẳn 1 repository riêng vì
    /// generic IRepository&lt;T&gt; không hỗ trợ Include, và đây là dữ liệu đọc cùng màn Brand Memory.</summary>
    Task<IReadOnlyList<ChannelAccount>> GetProjectChannelAccountsAsync(Guid projectId, CancellationToken cancellationToken = default);
}
