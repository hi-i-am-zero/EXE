using AutoWork.Application.DTOs.AI;

namespace AutoWork.Application.Interfaces.Services;

/// <summary>Vision AI: đọc ảnh sản phẩm, sinh mô tả chung, lưu vào Product.AiGeneratedDescription.
/// Tách riêng khỏi IAiContentService (text-only) theo nguyên tắc Single Responsibility.</summary>
public interface IProductVisionService
{
    Task<GenerateProductDescriptionResponse> GenerateProductDescriptionAsync(
        Guid userId, GenerateProductDescriptionRequest request, CancellationToken cancellationToken = default);
}
