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
        var creditCost = CreditCosts.GenerateProductContent;
        if (!await _creditService.HasSufficientCreditsAsync(userId, creditCost, cancellationToken))
        {
            throw new BadRequestException("Insufficient credits for AI product description generation.");
        }

        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var images = await _mediaFiles.FindAsync(m => m.ProductId == request.ProductId, cancellationToken);
        if (images.Count == 0)
        {
            throw new BadRequestException("Sản phẩm chưa có ảnh nào để AI phân tích. Vui lòng upload ảnh trước.");
        }

        var imageDataUris = new List<string>();
        foreach (var image in images.Take(4)) // giới hạn 4 ảnh/lần gọi để tránh vượt token limit và chi phí quá cao
        {
            var dataUri = await DownloadAsImageDataUriAsync(image.FileUrl, image.MimeType, cancellationToken);
            if (dataUri is not null)
            {
                imageDataUris.Add(dataUri);
            }
        }

        if (imageDataUris.Count == 0)
        {
            throw new BadRequestException("Không tải được ảnh sản phẩm để phân tích. Vui lòng thử lại.");
        }

        var provider = _providerFactory.GetProvider(request.Provider);
        var userPrompt = $"Tên sản phẩm hiện tại: {product.ProductName}. " +
                          "Hãy viết mô tả sản phẩm dựa trên (các) ảnh đính kèm.";

        var generated = new AiGeneratedContent
        {
            UserId = userId,
            Input = userPrompt,
            Status = (int)AiContentStatus.Processing
        };
        await _unitOfWork.Ai.AddAsync(generated, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await provider.GenerateAsync(new AiGenerationRequest
            {
                SystemPrompt = ProductVisionSystemPrompt,
                UserPrompt = userPrompt,
                ImageDataUris = imageDataUris,
                MaxTokens = 1024
            }, cancellationToken);

            product.AiGeneratedDescription = result.Content;
            await _products.UpdateAsync(product, cancellationToken);

            generated.Output = result.RawContent;
            generated.TokensUsed = result.TokensUsed;
            generated.Status = (int)AiContentStatus.Completed;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);

            await _creditService.DeductCreditsAsync(
                userId,
                creditCost,
                CreditTransactionType.GenerateProductContent,
                $"AI product description: {product.ProductName}",
                nameof(Product),
                product.Id,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GenerateProductDescriptionResponse
            {
                ProductId = product.Id,
                Description = result.Content,
                CreditsUsed = creditCost,
                TokensUsed = result.TokensUsed
            };
        }
        catch (Exception ex)
        {
            generated.Status = (int)AiContentStatus.Failed;
            generated.ErrorMessage = ex.Message;
            generated.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Ai.UpdateAsync(generated, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex, "AI product description generation failed for product {ProductId}", product.Id);
            throw;
        }
    }

    /// <summary>Tải ảnh từ FileUrl và chuyển thành base64 data URI để gửi cho AI vision.
    /// Luôn tải về base64 thay vì gửi thẳng URL — hoạt động nhất quán cho mọi provider kể cả khi
    /// URL nội bộ chưa public (VD: local storage chưa có CDN).</summary>
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
            _logger.LogWarning(ex, "Failed to download product image for AI vision: {FileUrl}", fileUrl);
            return null;
        }
    }
}
