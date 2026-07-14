using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

/// <summary>Tạo mới hoặc cập nhật Section 1 (Thông tin doanh nghiệp) + Section 2 (Địa chỉ liên hệ).</summary>
public class UpsertBrandProfileCoreCommand : IRequest<BrandProfileDto>
{
    public UpsertBrandProfileCoreDto Request { get; set; } = null!;
}
