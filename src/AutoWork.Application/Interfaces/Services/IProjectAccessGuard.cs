using AutoWork.Domain.Entities;

namespace AutoWork.Application.Interfaces.Services;

/// <summary>
/// Kiểm tra quyền của 1 user đối với 1 Project (workspace), dùng chung cho mọi Feature
/// thao tác trong phạm vi workspace (Brand Memory, Campaign, Timeline, Post, Members...).
/// Tránh lặp lại logic "user có phải Owner/Member của project không" ở từng Handler.
/// </summary>
public interface IProjectAccessGuard
{
    /// <summary>Project tồn tại và user có quyền XEM (Owner hoặc bất kỳ ProjectMember nào đang active).</summary>
    Task<Project> EnsureViewerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Project tồn tại và user có quyền SỬA (Owner, hoặc ProjectMember role Owner/Admin).</summary>
    Task<Project> EnsureEditorAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Chỉ true nếu user là Owner thật sự của project (dùng cho thao tác nhạy cảm: xoá thành viên, đổi role).</summary>
    Task<bool> IsOwnerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
