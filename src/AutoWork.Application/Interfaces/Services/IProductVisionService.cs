using AutoWork.Application.DTOs.AI;
using AutoWork.Shared.Enums;

namespace AutoWork.Application.Interfaces.Services;

/// <summary>Vision AI: đọc ảnh sản phẩm, sinh mô tả chung. Tách riêng khỏi IAiContentService
/// (text-only) theo nguyên tắc Single Responsibility.</summary>
public interface IProductVisionService
{
    /// <summary>Sinh mô tả cho 1 Product đã có trong Brand Memory (đọc ảnh đã gắn sẵn qua ProductId,
    /// lưu kết quả vào Product.AiGeneratedDescription).</summary>
    Task<GenerateProductDescriptionResponse> GenerateProductDescriptionAsync(
        Guid userId, GenerateProductDescriptionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sinh mô tả chung trực tiếp từ danh sách ảnh đã upload (MediaFileId), KHÔNG cần đã có
    /// Product — dùng cho bước 2 của luồng tạo Chiến dịch tự động (ảnh sản phẩm quảng bá trong chiến
    /// dịch, chưa chắc đã là 1 sản phẩm được catalogue trong Brand Memory).</summary>
    Task<(string Description, int CreditsUsed, int TokensUsed)> GenerateDescriptionFromImagesAsync(
        Guid userId, List<Guid> mediaFileIds, AiProvider provider, CancellationToken cancellationToken = default);
}
