using AutoWork.Application.DTOs.AI;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Features.BrandMemory.Commands;
using AutoWork.Application.Features.BrandMemory.Queries;
using AutoWork.Shared.Enums;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

/// <summary>
/// FlowMate v2: thay thế hoàn toàn cơ chế cũ (lưu JSON blob vào bảng Settings).
/// Toàn bộ dữ liệu Brand Memory giờ nằm ở entity thật (BrandProfile + các bảng con),
/// thao tác qua CQRS (MediatR Commands/Queries) đúng pattern của Features/Posts.
/// </summary>
[Authorize]
[Route("api/brand-memory")]
public class BrandMemoryController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public BrandMemoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Danh mục lookup dùng để render dropdown/checkbox (Styles, ToneKeywords, CTA, Hashtag, VoiceSample).</summary>
    [HttpGet("lookup-options")]
    public async Task<ActionResult<ApiResponse<BrandMemoryLookupOptionsDto>>> GetLookupOptions()
    {
        var result = await _mediator.Send(new GetBrandMemoryLookupOptionsQuery());
        return OkResponse(result);
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<BrandProfileDto>>> Get(Guid projectId)
    {
        var result = await _mediator.Send(new GetBrandProfileQuery { ProjectId = projectId });
        return OkResponse(result);
    }

    // ===== Section 1 + 2: Thông tin doanh nghiệp + Địa chỉ liên hệ =====
    [HttpPut("projects/{projectId:guid}/core")]
    public async Task<ActionResult<ApiResponse<BrandProfileDto>>> UpsertCore(
        Guid projectId, [FromBody] UpsertBrandProfileCoreDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new UpsertBrandProfileCoreCommand { Request = request });
        return OkResponse(result, "Đã lưu thông tin thương hiệu.");
    }

    // ===== Section 4: Sản phẩm =====
    [HttpPost("projects/{projectId:guid}/product-categories")]
    public async Task<ActionResult<ApiResponse<ProductCategoryDto>>> CreateProductCategory(
        Guid projectId, [FromBody] CreateProductCategoryDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new CreateProductCategoryCommand { Request = request });
        return OkResponse(result, "Đã thêm danh mục sản phẩm.");
    }

    [HttpPost("projects/{projectId:guid}/products")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(
        Guid projectId, [FromBody] CreateProductDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new CreateProductCommand { Request = request });
        return OkResponse(result, "Đã thêm sản phẩm.");
    }

    /// <summary>Vision AI: đọc ảnh sản phẩm đã gắn (qua CreateProduct.mediaFileIds), sinh mô tả chung,
    /// lưu vào Product.AiGeneratedDescription. Trừ credit theo CreditCosts.GenerateProductContent.</summary>
    [HttpPost("products/{productId:guid}/generate-description")]
    public async Task<ActionResult<ApiResponse<GenerateProductDescriptionResponse>>> GenerateProductDescription(
        Guid productId, [FromBody] GenerateProductDescriptionRequestDto request)
    {
        var result = await _mediator.Send(new GenerateProductDescriptionCommand
        {
            Request = new GenerateProductDescriptionRequest { ProductId = productId, Provider = request.Provider }
        });
        return OkResponse(result, "AI đã sinh mô tả sản phẩm.");
    }

    // ===== Section 5: Nhận diện thương hiệu =====
    [HttpPut("projects/{projectId:guid}/styles")]
    public async Task<ActionResult<ApiResponse<List<LookupItemDto>>>> SetStyles(
        Guid projectId, [FromBody] SetBrandTagsDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new SetBrandStylesCommand { Request = request });
        return OkResponse(result, "Đã cập nhật phong cách thương hiệu.");
    }

    [HttpPut("projects/{projectId:guid}/keywords")]
    public async Task<ActionResult<ApiResponse<List<LookupItemDto>>>> SetKeywords(
        Guid projectId, [FromBody] SetBrandTagsDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new SetBrandKeywordsCommand { Request = request });
        return OkResponse(result, "Đã cập nhật từ khóa giọng văn.");
    }

    [HttpPut("projects/{projectId:guid}/ctas")]
    public async Task<ActionResult<ApiResponse<List<LookupItemDto>>>> SetCtas(
        Guid projectId, [FromBody] SetBrandTagsDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new SetBrandCtasCommand { Request = request });
        return OkResponse(result, "Đã cập nhật CTA hay dùng.");
    }

    [HttpPut("projects/{projectId:guid}/hashtags")]
    public async Task<ActionResult<ApiResponse<List<LookupItemDto>>>> SetHashtags(
        Guid projectId, [FromBody] SetBrandTagsDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new SetBrandHashtagsCommand { Request = request });
        return OkResponse(result, "Đã cập nhật hashtag hay dùng.");
    }

    [HttpPut("projects/{projectId:guid}/colors")]
    public async Task<ActionResult<ApiResponse<List<BrandColorDto>>>> SetColors(
        Guid projectId, [FromBody] SetBrandColorsDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new SetBrandColorsCommand { Request = request });
        return OkResponse(result, "Đã cập nhật màu thương hiệu.");
    }

    [HttpPut("projects/{projectId:guid}/fonts")]
    public async Task<ActionResult<ApiResponse<List<BrandFontDto>>>> SetFonts(
        Guid projectId, [FromBody] SetBrandFontsDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new SetBrandFontsCommand { Request = request });
        return OkResponse(result, "Đã cập nhật font thương hiệu.");
    }
}

public class GenerateProductDescriptionRequestDto
{
    public AiProvider Provider { get; set; } = AiProvider.Claude;
}
