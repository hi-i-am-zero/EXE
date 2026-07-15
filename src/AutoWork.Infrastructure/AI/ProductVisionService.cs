using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Domain.Enums;
using AutoWork.Shared.Constants;
using AutoWork.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace AutoWork.Infrastructure.AI;

public class ProductVisionService : IProductVisionService
{
    private const string ProductVisionSystemPrompt = """
        You are a professional Vietnamese e-commerce copywriter. Look at the product photo(s) provided
        and write a general, appealing product description in Vietnamese suitable for a Facebook/Instagram
        shop post. Respond with valid JSON using this schema:
        {
          "title": "string (short product name/hook)",
          "description": "string (2-4 câu, mô tả chất liệu/kiểu dáng/công dụng dựa trên những gì nhìn thấy trong ảnh)",
          "content": "string (nội dung mô tả đầy đủ, có thể dùng trực tiếp làm mô tả sản phẩm)",
          "hashtags": "string"
        }
        """;

    private readonly AiProviderFactory _providerFactory;
    private readonly ICreditService _creditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Product> _products;
    private readonly IRepository<MediaFile> _mediaFiles;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductVisionService> _logger;

    public ProductVisionService(
        AiProviderFactory providerFactory,
        ICreditService creditService,
        IUnitOfWork unitOfWork,
        IRepository<Product> products,
        IRepository<MediaFile> mediaFiles,
        HttpClient httpClient,
        ILogger<ProductVisionService> logger)
    {
        _providerFactory = providerFactory;
        _creditService = creditService;
        _unitOfWork = unitOfWork;
        _products = products;
        _mediaFiles = mediaFiles;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GenerateProductDescriptionResponse> GenerateProductDescriptionAsync(
        Guid userId,
        GenerateProductDescriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var images = await _mediaFiles.FindAsync(m => m.ProductId == request.ProductId, cancellationToken);
        if (images.Count == 0)
        {
            throw new BadRequestException("Sản phẩm chưa có ảnh nào để AI phân tích. Vui lòng upload ảnh trước.");
        }

        var contextPrompt = $"Tên sản phẩm hiện tại: {product.ProductName}. " +
                             "Hãy viết mô tả sản phẩm dựa trên (các) ảnh đính kèm.";

        var (description, rawContent, creditsUsed, tokensUsed) = await GenerateVisionDescriptionCoreAsync(
            userId, images.Select(i => i.FileUrl).ToList(), images.Select(i => i.MimeType).ToList(),
            contextPrompt, request.Provider, nameof(Product), product.Id, cancellationToken);

        product.AiGeneratedDescription = description;
        await _products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new GenerateProductDescriptionResponse
        {
            ProductId = product.Id,
            Description = description,
            CreditsUsed = creditsUsed,
            TokensUsed = tokensUsed
        };
    }

    public async Task<(string Description, int CreditsUsed, int TokensUsed)> GenerateDescriptionFromImagesAsync(
        Guid userId, List<Guid> mediaFileIds, AiProvider provider, CancellationToken cancellationToken = default)
    {
        if (mediaFileIds.Count == 0)
        {
            throw new BadRequestException("Vui lòng upload ít nhất 1 ảnh sản phẩm để AI phân tích.");
        }

        var images = await _mediaFiles.FindAsync(m => mediaFileIds.Contains(m.Id), cancellationToken);
        if (images.Count == 0)
        {
            throw new BadRequestException("Không tìm thấy ảnh đã upload. Vui lòng thử lại.");
        }

        const string contextPrompt = "Hãy viết mô tả chung, hấp dẫn cho sản phẩm dựa trên (các) ảnh đính kèm — " +
                                      "dùng để AI hiểu về sản phẩm khi lên nội dung quảng bá cho chiến dịch.";

        var (description, _, creditsUsed, tokensUsed) = await GenerateVisionDescriptionCoreAsync(
            userId, images.Select(i => i.FileUrl).ToList(), images.Select(i => i.MimeType).ToList(),
            contextPrompt, provider, "Campaign", null, cancellationToken);

        return (description, creditsUsed, tokensUsed);
    }

    /// <summary>Phần lõi dùng chung cho cả 2 method public phía trên: tải ảnh, gọi AI vision, trừ credit,
    /// ghi log AiGeneratedContent. Tách ra để không lặp lại logic giữa "mô tả cho Product đã catalogue"
    /// và "mô tả nhanh cho ảnh chiến dịch chưa gắn Product".</summary>
    private async Task<(string Description, string RawContent, int CreditsUsed, int TokensUsed)> GenerateVisionDescriptionCoreAsync(
        Guid userId,
        List<string> fileUrls,
        List<string> mimeTypes,
        string contextPrompt,
        AiProvider provider,
        string referenceType,
        Guid? referenceId,
        CancellationToken cancellationToken)
    {
        var creditCost = CreditCosts.GenerateProductContent;
        if (!await _creditService.HasSufficientCreditsAsync(userId, creditCost, cancellationToken))
        {
            throw new BadRequestException("Insufficient credits for AI vision generation.");
        }

        var imageDataUris = new List<string>();
        for (var i = 0; i < fileUrls.Count && imageDataUris.Count < 4; i++) // tối đa 4 ảnh/lần gọi
        {
            var dataUri = await DownloadAsImageDataUriAsync(fileUrls[i], mimeTypes.ElementAtOrDefault(i) ?? "image/jpeg", cancellationToken);
            if (dataUri is not null)
            {
                imageDataUris.Add(dataUri);
            }
        }

        if (imageDataUris.Count == 0)
        {
            throw new BadRequestException("Không tải được ảnh để phân tích. Vui lòng thử lại.");
        }

        var aiProvider = _providerFactory.GetProvider(provider);

        var generated = new AiGeneratedContent
        {
            UserId = userId,
            Input = contextPrompt,
            Status = (int)AiContentStatus.Processing
        };
        await _unitOfWork.Ai.AddAsync(generated, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await aiProvider.GenerateAsync(new AiGenerationRequest
            {
                SystemPrompt = ProductVisionSystemPrompt,
                UserPrompt = contextPrompt,
                ImageDataUris = imageDataUris,
                MaxTokens = 1024
            }, cancellationToken);

            generated.Output = result.RawContent;
            generated.TokensUsed = result.TokensUsed;
            generated.Status = (int)AiContentStatus.Completed;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);

            await _creditService.DeductCreditsAsync(
                userId, creditCost, AutoWork.Shared.Enums.CreditTransactionType.GenerateProductContent,
                "AI vision description generation", referenceType, referenceId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return (result.Content, result.RawContent, creditCost, result.TokensUsed);
        }
        catch (Exception ex)
        {
            generated.Status = (int)AiContentStatus.Failed;
            generated.ErrorMessage = ex.Message;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex, "AI vision generation failed for {ReferenceType} {ReferenceId}", referenceType, referenceId);
            throw;
        }
    }

    private async Task<string?> DownloadAsImageDataUriAsync(string fileUrl, string mimeType, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(fileUrl, cancellationToken);
            var base64 = Convert.ToBase64String(bytes);
            var mediaType = string.IsNullOrWhiteSpace(mimeType) ? "image/jpeg" : mimeType;
            return $"data:{mediaType};base64,{base64}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to download image for AI vision: {FileUrl}", fileUrl);
            return null;
        }
    }
}
